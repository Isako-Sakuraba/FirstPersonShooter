using Game.Data;
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Game.Weapons.Animation
{
    internal sealed class WeaponAnimationPlayer<T> :
        AnimationLayerBase<T>,
        IAnimationLayer
        where T : Enum
    {
        private readonly PlayableGraph _graph;
        private readonly AnimationPlayableOutput _output;

        public WeaponAnimationPlayer(
            Animator weaponAnimator,
            AnimationSet<T> animationSet)
        {
            _graph = PlayableGraph.Create($"WeaponGraph_{weaponAnimator.name}_{typeof(T).Name}");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            _output = AnimationPlayableOutput.Create(_graph, "WeaponOutput", weaponAnimator);

            Initialize(
                _graph,
                animationSet,
                idle => idle.WeaponClip,
                animation => animation.WeaponClip);

            _output.SetSourcePlayable(TopMixer);

            _graph.Play();
            ApplyActiveState(false);
        }

        protected override void ApplyActiveState(bool active)
        {
            if (!_graph.IsValid())
                return;

            Playable root = _graph.GetRootPlayable(0);
            if (root.IsValid())
                root.SetSpeed(active ? 1d : 0d);
        }

        protected override void DestroyInternal()
        {
            if (_graph.IsValid())
                _graph.Destroy();
        }
    }
}