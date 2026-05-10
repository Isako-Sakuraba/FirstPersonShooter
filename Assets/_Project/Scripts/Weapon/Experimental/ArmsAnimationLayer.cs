using Game.Data;
using System;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Game.Weapons.Animation
{
    internal sealed class ArmsAnimationLayer<T> :
        AnimationLayerBase<T>,
        IAnimationLayer
        where T : Enum
    {
        private readonly AnimationMixerPlayable _armsRootMixer;
        private readonly int _armsSlotIndex;
        private readonly PlayableGraph _graph;

        public ArmsAnimationLayer(
            PlayableGraph graph,
            AnimationMixerPlayable armsRootMixer,
            int armsSlotIndex,
            AnimationSet<T> animationSet)
        {
            _graph = graph;
            _armsRootMixer = armsRootMixer;
            _armsSlotIndex = armsSlotIndex;

            Initialize(
                _graph,
                animationSet,
                idle => idle.ArmsClip,
                animation => animation.ArmsClip);

            _graph.Connect(TopMixer, 0, _armsRootMixer, _armsSlotIndex);
            _armsRootMixer.SetInputWeight(_armsSlotIndex, 0f);
        }

        protected override void ApplyActiveState(bool active)
        {
            _armsRootMixer.SetInputWeight(_armsSlotIndex, active ? 1f : 0f);
        }

        protected override void DestroyInternal()
        {
            if (!_graph.IsValid())
                return;

            _armsRootMixer.DisconnectInput(_armsSlotIndex);
            _graph.DestroySubgraph(TopMixer);
        }
    }
}