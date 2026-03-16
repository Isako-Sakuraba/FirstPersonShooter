using Game.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Game.Weapons.Animation
{
    internal abstract class AnimationLayerBase<T> where T : Enum
    {
        private readonly struct ActionRuntimeData
        {
            public readonly int InputIndex;
            public readonly float ClipLength;
            public readonly float FadeInDuration;
            public readonly float FadeOutToIdleDuration;

            public ActionRuntimeData(
                int inputIndex,
                float clipLength,
                float fadeInDuration,
                float fadeOutToIdleDuration)
            {
                InputIndex = inputIndex;
                ClipLength = clipLength;
                FadeInDuration = fadeInDuration;
                FadeOutToIdleDuration = fadeOutToIdleDuration;
            }
        }

        private readonly Dictionary<T, ActionRuntimeData> _actionsById = new();

        protected PlayableGraph Graph { get; private set; }
        protected AnimationMixerPlayable TopMixer { get; private set; }
        protected AnimationMixerPlayable ActionMixer { get; private set; }

        private bool _isInitialized;

        private bool _hasIdle;
        private float _defaultIdleEnterFadeDuration;

        private int _currentActionInputIndex = -1;
        private int _previousActionInputIndex = -1;

        private bool _isActionCrossfading;
        private float _actionCrossfadeDuration;
        private float _actionCrossfadeElapsed;

        private bool _isTopBlendActive;
        private float _topBlendDuration;
        private float _topBlendElapsed;
        private float _topBlendStartIdleWeight;
        private float _topBlendTargetIdleWeight;

        private bool _isPlayingOneShot;
        private float _oneShotRemaining;
        private float _currentExitToIdleFadeDuration;

        public Type AnimationIdType => typeof(T);

        protected void Initialize(
            PlayableGraph graph,
            AnimationSet<T> animationSet,
            Func<AnimationSet<T>.IdleEntry, AnimationClip> idleClipSelector,
            Func<AnimationSet<T>.Entry, AnimationClip> actionClipSelector)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{GetType().Name} is already initialized.");

            Graph = graph;

            int actionInputCount = Math.Max(CountValidActionClips(animationSet, actionClipSelector), 1);

            TopMixer = AnimationMixerPlayable.Create(Graph, 2);
            ActionMixer = AnimationMixerPlayable.Create(Graph, actionInputCount);

            Graph.Connect(ActionMixer, 0, TopMixer, 1);

            AnimationClip idleClip = idleClipSelector(animationSet.Idle);
            _hasIdle = idleClip != null;
            _defaultIdleEnterFadeDuration = animationSet.Idle.EnterFadeDuration;

            if (_hasIdle)
            {
                AnimationClipPlayable idlePlayable = AnimationClipPlayable.Create(Graph, idleClip);
                idlePlayable.SetApplyFootIK(false);
                idlePlayable.SetApplyPlayableIK(false);
                idlePlayable.Play();

                Graph.Connect(idlePlayable, 0, TopMixer, 0);
                TopMixer.SetInputWeight(0, 1f);
                TopMixer.SetInputWeight(1, 0f);
            }
            else
            {
                TopMixer.SetInputWeight(0, 0f);
                TopMixer.SetInputWeight(1, 1f);
            }

            BuildActionInputs(animationSet, actionClipSelector);
            _isInitialized = true;
        }

        public bool Play(Enum id)
        {
            if (id is not T typedId)
                return false;

            if (!_actionsById.TryGetValue(typedId, out ActionRuntimeData action))
                return false;

            _isPlayingOneShot = false;

            StartAction(action, restartClip: true);
            StartTopBlendToAction(action.FadeInDuration);

            _currentExitToIdleFadeDuration = action.FadeOutToIdleDuration;
            return true;
        }

        public bool PlayOneShot(Enum id)
        {
            if (id is not T typedId)
                return false;

            if (!_actionsById.TryGetValue(typedId, out ActionRuntimeData action))
                return false;

            _isPlayingOneShot = true;
            _oneShotRemaining = action.ClipLength;

            StartAction(action, restartClip: true);
            StartTopBlendToAction(action.FadeInDuration);

            _currentExitToIdleFadeDuration = action.FadeOutToIdleDuration;
            return true;
        }

        public void ResetToIdle(float? fadeOverride = null)
        {
            _isPlayingOneShot = false;
            _oneShotRemaining = 0f;

            if (!_hasIdle)
            {
                ClearActionWeights();
                _currentActionInputIndex = -1;
                _previousActionInputIndex = -1;
                return;
            }

            float fade = fadeOverride ?? _currentExitToIdleFadeDuration;
            if (fade <= 0f)
                fade = _defaultIdleEnterFadeDuration;

            StartTopBlendToIdle(fade);
        }

        public void Tick(float deltaTime)
        {
            TickActionCrossfade(deltaTime);
            TickTopBlend(deltaTime);
            TickOneShot(deltaTime);
        }

        protected abstract void ApplyActiveState(bool active);
        protected abstract void DestroyInternal();

        public void SetActive(bool active)
        {
            ApplyActiveState(active);
        }

        public void Destroy()
        {
            DestroyInternal();
        }

        private void BuildActionInputs(
            AnimationSet<T> animationSet,
            Func<AnimationSet<T>.Entry, AnimationClip> actionClipSelector)
        {
            int inputIndex = 0;

            foreach (AnimationSet<T>.Entry animation in animationSet.Entries)
            {
                AnimationClip clip = actionClipSelector(animation);
                if (clip == null)
                    continue;

                if (_actionsById.ContainsKey(animation.Id))
                {
                    Debug.LogWarning(
                        $"Duplicate animation id '{animation.Id}' in set '{animationSet.name}'.");
                    continue;
                }

                AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(Graph, clip);
                clipPlayable.SetApplyFootIK(false);
                clipPlayable.SetApplyPlayableIK(false);
                clipPlayable.Pause();

                Graph.Connect(clipPlayable, 0, ActionMixer, inputIndex);
                ActionMixer.SetInputWeight(inputIndex, 0f);

                _actionsById.Add(
                    animation.Id,
                    new ActionRuntimeData(
                        inputIndex,
                        clip.length,
                        animation.FadeInDuration,
                        animation.FadeOutToIdleDuration));

                inputIndex++;
            }
        }

        private static int CountValidActionClips(
            AnimationSet<T> animationSet,
            Func<AnimationSet<T>.Entry, AnimationClip> actionClipSelector)
        {
            int count = 0;

            foreach (AnimationSet<T>.Entry animation in animationSet.Entries)
            {
                if (actionClipSelector(animation) != null)
                    count++;
            }

            return count;
        }

        private void StartAction(ActionRuntimeData action, bool restartClip)
        {
            int nextIndex = action.InputIndex;

            if (_currentActionInputIndex == nextIndex)
            {
                if (restartClip)
                    RestartActionClip(nextIndex);

                if (_previousActionInputIndex >= 0)
                {
                    ActionMixer.SetInputWeight(_previousActionInputIndex, 0f);
                    _previousActionInputIndex = -1;
                }

                ActionMixer.SetInputWeight(_currentActionInputIndex, 1f);
                _isActionCrossfading = false;
                return;
            }

            if (_currentActionInputIndex >= 0)
            {
                _previousActionInputIndex = _currentActionInputIndex;
                _currentActionInputIndex = nextIndex;

                RestartActionClip(_currentActionInputIndex);

                _actionCrossfadeElapsed = 0f;
                _actionCrossfadeDuration = Mathf.Max(0f, action.FadeInDuration);

                if (_actionCrossfadeDuration <= 0f)
                {
                    ActionMixer.SetInputWeight(_previousActionInputIndex, 0f);
                    ActionMixer.SetInputWeight(_currentActionInputIndex, 1f);
                    _previousActionInputIndex = -1;
                    _isActionCrossfading = false;
                }
                else
                {
                    _isActionCrossfading = true;
                    ActionMixer.SetInputWeight(_previousActionInputIndex, 1f);
                    ActionMixer.SetInputWeight(_currentActionInputIndex, 0f);
                }
            }
            else
            {
                ClearActionWeights();

                _currentActionInputIndex = nextIndex;
                _previousActionInputIndex = -1;

                RestartActionClip(_currentActionInputIndex);
                ActionMixer.SetInputWeight(_currentActionInputIndex, 1f);
                _isActionCrossfading = false;
            }
        }

        private void TickActionCrossfade(float deltaTime)
        {
            if (!_isActionCrossfading)
                return;

            if (_previousActionInputIndex < 0 || _currentActionInputIndex < 0)
            {
                _isActionCrossfading = false;
                return;
            }

            _actionCrossfadeElapsed += deltaTime;
            float t = _actionCrossfadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(_actionCrossfadeElapsed / _actionCrossfadeDuration);

            ActionMixer.SetInputWeight(_previousActionInputIndex, 1f - t);
            ActionMixer.SetInputWeight(_currentActionInputIndex, t);

            if (t >= 1f)
            {
                ActionMixer.SetInputWeight(_previousActionInputIndex, 0f);
                ActionMixer.SetInputWeight(_currentActionInputIndex, 1f);

                _previousActionInputIndex = -1;
                _isActionCrossfading = false;
            }
        }

        private void StartTopBlendToAction(float duration)
        {
            if (!_hasIdle)
            {
                TopMixer.SetInputWeight(0, 0f);
                TopMixer.SetInputWeight(1, 1f);
                _isTopBlendActive = false;
                return;
            }

            StartTopBlend(targetIdleWeight: 0f, duration);
        }

        private void StartTopBlendToIdle(float duration)
        {
            if (!_hasIdle)
            {
                TopMixer.SetInputWeight(0, 0f);
                TopMixer.SetInputWeight(1, 0f);
                _isTopBlendActive = false;
                return;
            }

            StartTopBlend(targetIdleWeight: 1f, duration);
        }

        private void StartTopBlend(float targetIdleWeight, float duration)
        {
            _topBlendStartIdleWeight = TopMixer.GetInputWeight(0);
            _topBlendTargetIdleWeight = Mathf.Clamp01(targetIdleWeight);

            _topBlendElapsed = 0f;
            _topBlendDuration = Mathf.Max(0f, duration);

            if (_topBlendDuration <= 0f)
            {
                ApplyTopBlendWeights(_topBlendTargetIdleWeight);
                _isTopBlendActive = false;

                if (Mathf.Approximately(_topBlendTargetIdleWeight, 1f))
                    FinishReturnToIdle();

                return;
            }

            _isTopBlendActive = true;
        }

        private void TickTopBlend(float deltaTime)
        {
            if (!_isTopBlendActive)
                return;

            _topBlendElapsed += deltaTime;
            float t = Mathf.Clamp01(_topBlendElapsed / _topBlendDuration);

            float idleWeight = Mathf.Lerp(
                _topBlendStartIdleWeight,
                _topBlendTargetIdleWeight,
                t);

            ApplyTopBlendWeights(idleWeight);

            if (t >= 1f)
            {
                _isTopBlendActive = false;

                if (Mathf.Approximately(_topBlendTargetIdleWeight, 1f))
                    FinishReturnToIdle();
            }
        }

        private void TickOneShot(float deltaTime)
        {
            if (!_isPlayingOneShot)
                return;

            _oneShotRemaining -= deltaTime;
            if (_oneShotRemaining > 0f)
                return;

            _isPlayingOneShot = false;
            _oneShotRemaining = 0f;

            ResetToIdle(_currentExitToIdleFadeDuration);
        }

        private void FinishReturnToIdle()
        {
            ClearActionWeights();
            _currentActionInputIndex = -1;
            _previousActionInputIndex = -1;
            _isActionCrossfading = false;
        }

        private void ApplyTopBlendWeights(float idleWeight)
        {
            float clampedIdleWeight = Mathf.Clamp01(idleWeight);
            TopMixer.SetInputWeight(0, clampedIdleWeight);
            TopMixer.SetInputWeight(1, 1f - clampedIdleWeight);
        }

        private void RestartActionClip(int inputIndex)
        {
            Playable input = ActionMixer.GetInput(inputIndex);
            if (!input.IsValid() || !input.IsPlayableOfType<AnimationClipPlayable>())
                return;

            AnimationClipPlayable clipPlayable = (AnimationClipPlayable)input;
            clipPlayable.SetTime(0d);
            clipPlayable.Play();
        }

        private void ClearActionWeights()
        {
            int inputCount = ActionMixer.GetInputCount();
            for (int i = 0; i < inputCount; i++)
                ActionMixer.SetInputWeight(i, 0f);
        }
    }
}