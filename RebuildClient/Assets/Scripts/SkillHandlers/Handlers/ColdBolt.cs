﻿using Assets.Scripts.Effects;
using Assets.Scripts.Effects.EffectHandlers;
using Assets.Scripts.Network;
using JetBrains.Annotations;
using RebuildSharedData.Enum;

namespace Assets.Scripts.SkillHandlers.Handlers
{
    [SkillHandler(CharacterSkill.ColdBolt, true)]
    public class ColdBoltHandler : SkillHandlerBase
    {
        public override void StartSkillCasting(ServerControllable src, ServerControllable target, SkillTarget skillType, int lvl, float castTime)
        {
            if (src.SpriteAnimator.State != SpriteState.Dead && src.SpriteAnimator.State != SpriteState.Walking)
            {
                src.SpriteAnimator.State = SpriteState.Standby;
                src.SpriteAnimator.ChangeMotion(SpriteMotion.Standby);
            }
            src.AttachEffect(CastEffect.Create(castTime, "ring_blue", src.gameObject, true));
            
            if(target != null)
                target.AttachEffect(CastLockOnEffect.Create(castTime, target.gameObject));
        }

        public override void InterruptSkillCasting(ServerControllable src)
        {
            src.EndEffectOfType(EffectType.CastEffect);
        }

        public override void ExecuteSkillTargeted([CanBeNull] ServerControllable src, ServerControllable target, int lvl)
        {
            src?.PerformBasicAttackMotion();
            if(target != null)
                IceArrow.Create(src, target, lvl); //don't attach to the entity so the effect stays if they get removed
        }
    }
}