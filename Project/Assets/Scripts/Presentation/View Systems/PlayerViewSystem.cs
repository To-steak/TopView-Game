using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Presentation.ViewContexts;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 플레이어 View를 만들고, 위치·회전 보간, Animator 파라미터, 무기 모델, 공격·점프·회피 효과음을 엔티티에 맞춘다.
    public static class PlayerViewSystem
    {
        private static readonly int MOVEMENT_SPEED_HASH = Animator.StringToHash("MovementSpeed");
        private static readonly int IS_JUMP_HASH = Animator.StringToHash("IsJump");
        private static readonly int DODGE_HASH = Animator.StringToHash("Dodge");
        private static readonly int SHOT_HASH = Animator.StringToHash("Shot");
        private static readonly int SWING_HASH = Animator.StringToHash("Swing");
        private static readonly int SWAP_HASH = Animator.StringToHash("Swap");
        private static readonly int RELOAD_HASH = Animator.StringToHash("Reload");
        private static readonly int THROW_HASH = Animator.StringToHash("Throw");
        private static readonly int DIE_HASH = Animator.StringToHash("Die");
        private static readonly int[] TRIGGER_HASHES = { DODGE_HASH, SHOT_HASH, SWING_HASH, SWAP_HASH, RELOAD_HASH, THROW_HASH, DIE_HASH };

        // Ranged Layer의 상태 이름
        private static readonly int SHOT_IDLE_STATE_HASH = Animator.StringToHash("Shot Idle");
        private static readonly int RELOAD_STATE_HASH = Animator.StringToHash("Reload");

        // MovementSpeed 파라미터 값
        private const float IDLE_SPEED = 0f;
        private const float WALK_SPEED = 1f;
        private const float RUN_SPEED = 2f;

        private const string RANGED_LAYER_NAME = "Ranged Layer";
        private const string ACTION_LAYER_NAME = "Action Layer";
        private const float LAYER_ON_WEIGHT = 1f;
        private const float LAYER_OFF_WEIGHT = 0f;

        // 장전이 끊겼을 때 Ranged Layer가 Reload에서 Shot Idle로 넘어가는 시간(초)
        private const float RELOAD_INTERRUPT_CROSS_FADE_SECONDS = 0.1f;

        /// <summary>
        /// 살아 있는 플레이어가 있으면 View를 만들거나 맞추고, 없으면 View를 끈다.
        /// </summary>
        /// <param name="entities">플레이어 엔티티 배열</param>
        /// <param name="stateRecords">이번 프레임의 상태 변경 기록</param>
        /// <param name="attackRecords">이번 프레임의 공격을 내보낸 기록</param>
        /// <param name="viewPrefab">PlayerView가 붙은 프리팹</param>
        /// <param name="weaponModelPrefabs">무기 번호 순서의 무기 모델 프리팹. 없는 칸은 null</param>
        /// <param name="rangedLayerBlendInTicks">원거리 레이어 가중치가 0에서 1로 올라오는 시간(틱)</param>
        /// <param name="isPaused">판이 일시정지 중이면 true. Animator와 원거리 레이어 가중치를 멈춘다</param>
        /// <param name="context">플레이어 View 실행 상태</param>
        public static void Present(Entity[] entities, List<PlayerStateChangedRecord> stateRecords, List<PlayerAttackCommittedRecord> attackRecords, GameObject viewPrefab, GameObject[] weaponModelPrefabs, int rangedLayerBlendInTicks, bool isPaused, PlayerViewContext context)
        {
            if (!TryFindAlivePlayer(entities, out int playerIndex))
            {
                if (context.view != null && context.view.gameObject.activeSelf)
                {
                    context.view.gameObject.SetActive(false);
                }
                return;
            }

            ref readonly Entity entity = ref entities[playerIndex];
            ref readonly PositionComponent position = ref entity.position;
            ref readonly RotationComponent rotation = ref entity.rotation;
            ref readonly PlayerActionStateComponent actionState = ref entity.playerActionState;
            ref readonly PlayerWeaponSlotsComponent weaponSlot = ref entity.weaponSlots;
            EntityIdentifier identifier = new EntityIdentifier { index = playerIndex, generation = entity.generation };

            if (context.view == null)
            {
                CreateView(in position, in rotation, viewPrefab, weaponModelPrefabs, context);
            }
            if (!context.view.gameObject.activeSelf)
            {
                context.view.gameObject.SetActive(true);
            }

            Animator animator = context.view.animator;
            animator.speed = isPaused ? 0f : 1f;
            ApplyStateRecords(in identifier, stateRecords, animator, context.rangedLayerIndex);
            PlayAttackEffects(in identifier, attackRecords, context.weaponViews, context.view.soundEffectSource);
            PlayStateSounds(in identifier, stateRecords, context.view);
            PlayReloadSounds(in identifier, stateRecords, in actionState, weaponSlot.currentIndex, context);
            ApplyMovementAnimation(in actionState, in entity.playerMotion, animator);
            // 가중치는 프레임 시간만큼 목표로 다가가므로, 일시정지 중에는 Animator와 함께 멈춘다
            if (!isPaused)
            {
                ApplyRangedLayerWeight(in actionState, in weaponSlot, entities, animator, context.rangedLayerIndex, rangedLayerBlendInTicks);
            }
            ApplyActionLayerWeight(in actionState, animator, context.actionLayerIndex);
            ApplyWeaponModels(in weaponSlot, context.weaponModels);
            ApplyTransform(in position, in rotation, context.view.transform);
        }

        private static bool TryFindAlivePlayer(Entity[] entities, out int playerIndex)
        {
            for (playerIndex = 0; playerIndex < entities.Length; playerIndex++)
            {
                if ((entities[playerIndex].mask & ComponentMask.PlayerActionState) != 0)
                {
                    return true;
                }
            }

            playerIndex = -1;
            return false;
        }

        private static void CreateView(in PositionComponent position, in RotationComponent rotation, GameObject viewPrefab, GameObject[] weaponModelPrefabs, PlayerViewContext context)
        {
            GameObject viewObject = Object.Instantiate(viewPrefab, position.current, rotation.current);
            context.view = viewObject.GetComponent<PlayerView>();
            context.rangedLayerIndex = FindLayerIndex(context.view.animator, RANGED_LAYER_NAME);
            context.actionLayerIndex = FindLayerIndex(context.view.animator, ACTION_LAYER_NAME);

            if (context.view.soundEffectSource == null)
            {
                Debug.LogError("PlayerViewSystem >> PlayerView에 Sound Effect Source가 연결되어 있지 않아 공격 효과음이 나오지 않는다.");
            }
            if (context.view.jumpClip == null)
            {
                Debug.LogWarning("PlayerViewSystem >> PlayerView에 Jump Clip이 없어 점프 효과음이 나오지 않는다.");
            }
            if (context.view.dodgeClip == null)
            {
                Debug.LogWarning("PlayerViewSystem >> PlayerView에 Dodge Clip이 없어 회피 효과음이 나오지 않는다.");
            }
            if (context.view.throwPreviewLine == null || context.view.throwPreviewCircle == null)
            {
                Debug.LogError("PlayerViewSystem >> PlayerView에 Throw Preview Line이나 Throw Preview Circle이 연결되어 있지 않아 투척 미리보기 일부가 보이지 않는다.");
            }

            context.weaponModels = new GameObject[weaponModelPrefabs.Length];
            context.weaponViews = new WeaponView[weaponModelPrefabs.Length];
            for (int weaponIndex = 0; weaponIndex < weaponModelPrefabs.Length; weaponIndex++)
            {
                if (weaponModelPrefabs[weaponIndex] == null)
                {
                    continue;
                }

                GameObject model = Object.Instantiate(weaponModelPrefabs[weaponIndex], context.view.weaponSocket);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.SetActive(false);
                context.weaponModels[weaponIndex] = model;

                WeaponView weaponView = model.GetComponent<WeaponView>();
                if (weaponView == null)
                {
                    Debug.LogError($"PlayerViewSystem >> {weaponModelPrefabs[weaponIndex].name} 무기 모델 프리팹에 WeaponView가 없어 이 무기의 공격 효과음이 나오지 않는다.");
                }
                else if (weaponView.attackClip == null)
                {
                    Debug.LogWarning($"PlayerViewSystem >> {weaponModelPrefabs[weaponIndex].name}의 WeaponView에 Attack Clip이 없어 이 무기의 공격 효과음이 나오지 않는다.");
                }
                if (weaponView != null)
                {
                    WarnInvalidReloadSounds(weaponView.reloadSounds, weaponModelPrefabs[weaponIndex].name);
                }
                context.weaponViews[weaponIndex] = weaponView;
            }
        }

        // 장전 소리 중 Clip이 빠졌거나 startTicks가 오름차순이 아닌 칸을 알린다. 오름차순이 아니면 뒤 칸이 앞 칸의 시점까지 기다렸다가 함께 재생된다.
        private static void WarnInvalidReloadSounds(TimedSoundEntry[] reloadSounds, string prefabName)
        {
            if (reloadSounds == null)
            {
                return;
            }

            for (int soundIndex = 0; soundIndex < reloadSounds.Length; soundIndex++)
            {
                if (reloadSounds[soundIndex].clip == null)
                {
                    Debug.LogWarning($"PlayerViewSystem >> {prefabName}의 WeaponView Reload Sounds {soundIndex}번 칸에 Clip이 없어 그 칸은 소리를 내지 않는다.");
                }
                if (soundIndex > 0 && reloadSounds[soundIndex].startTicks < reloadSounds[soundIndex - 1].startTicks)
                {
                    Debug.LogWarning($"PlayerViewSystem >> {prefabName}의 WeaponView Reload Sounds {soundIndex}번 칸의 Start Ticks({reloadSounds[soundIndex].startTicks})가 앞 칸({reloadSounds[soundIndex - 1].startTicks})보다 작아 앞 칸의 시점에 함께 재생된다.");
                }
            }
        }

        // 이 플레이어가 공격을 내보낸 기록마다 그 무기의 공격 Clip을 겹쳐 재생하고, Impulse Source가 있으면 화면 흔들림을 낸다. 연결이 빠진 경우는 View를 만들 때 이미 알렸다.
        private static void PlayAttackEffects(in EntityIdentifier identifier, List<PlayerAttackCommittedRecord> attackRecords, WeaponView[] weaponViews, AudioSource soundEffectSource)
        {
            for (int recordIndex = 0; recordIndex < attackRecords.Count; recordIndex++)
            {
                PlayerAttackCommittedRecord record = attackRecords[recordIndex];
                if (record.player.index != identifier.index || record.player.generation != identifier.generation)
                {
                    continue;
                }

                if (record.weaponIndex < 0 || record.weaponIndex >= weaponViews.Length)
                {
                    Debug.LogWarning($"PlayerViewSystem >> 공격 기록의 무기 번호({record.weaponIndex})가 무기 수({weaponViews.Length}) 밖이라 공격 효과음을 재생하지 않았다.");
                    continue;
                }

                WeaponView weaponView = weaponViews[record.weaponIndex];
                if (weaponView == null)
                {
                    continue;
                }

                if (soundEffectSource != null && weaponView.attackClip != null)
                {
                    soundEffectSource.PlayOneShot(weaponView.attackClip);
                }
                if (weaponView.attackImpulseSource != null)
                {
                    weaponView.attackImpulseSource.GenerateImpulse();
                }
            }
        }

        // 이 플레이어가 점프나 회피 상태에 들어간 기록마다 그 소리를 겹쳐 재생한다. 연결이 빠진 경우는 View를 만들 때 이미 알렸다.
        private static void PlayStateSounds(in EntityIdentifier identifier, List<PlayerStateChangedRecord> stateRecords, PlayerView view)
        {
            AudioSource soundEffectSource = view.soundEffectSource;
            if (soundEffectSource == null)
            {
                return;
            }

            for (int recordIndex = 0; recordIndex < stateRecords.Count; recordIndex++)
            {
                PlayerStateChangedRecord record = stateRecords[recordIndex];
                if (record.player.index != identifier.index || record.player.generation != identifier.generation)
                {
                    continue;
                }

                AudioClip clip;
                switch (record.state)
                {
                    case PlayerState.Jump:
                        clip = view.jumpClip;
                        break;
                    case PlayerState.Dodge:
                        clip = view.dodgeClip;
                        break;
                    default:
                        continue;
                }

                if (clip != null)
                {
                    soundEffectSource.PlayOneShot(clip);
                }
            }
        }

        // 장전 상태에 들어간 기록이 있으면 장전 소리를 처음부터 다시 센다. 장전 중에는 경과 틱이 startTicks에 닿은 소리를 차례로 재생한다.
        // 장전이 끊기면 남은 소리는 재생하지 않고, 이미 재생 중인 소리만 끝까지 난다.
        private static void PlayReloadSounds(in EntityIdentifier identifier, List<PlayerStateChangedRecord> stateRecords, in PlayerActionStateComponent actionState, int currentWeaponIndex, PlayerViewContext context)
        {
            for (int recordIndex = 0; recordIndex < stateRecords.Count; recordIndex++)
            {
                PlayerStateChangedRecord record = stateRecords[recordIndex];
                if (record.state == PlayerState.Reload && record.player.index == identifier.index && record.player.generation == identifier.generation)
                {
                    context.nextReloadSoundIndex = 0;
                }
            }

            AudioSource soundEffectSource = context.view.soundEffectSource;
            if (actionState.state != PlayerState.Reload || soundEffectSource == null)
            {
                return;
            }

            if (currentWeaponIndex < 0 || currentWeaponIndex >= context.weaponViews.Length)
            {
                Debug.LogWarning($"PlayerViewSystem >> 들고 있는 무기 번호({currentWeaponIndex})가 무기 수({context.weaponViews.Length}) 밖이라 장전 효과음을 재생하지 않았다.");
                return;
            }

            WeaponView weaponView = context.weaponViews[currentWeaponIndex];
            if (weaponView == null || weaponView.reloadSounds == null)
            {
                return;
            }

            TimedSoundEntry[] reloadSounds = weaponView.reloadSounds;
            while (context.nextReloadSoundIndex < reloadSounds.Length && reloadSounds[context.nextReloadSoundIndex].startTicks <= actionState.elapsedTicks)
            {
                AudioClip clip = reloadSounds[context.nextReloadSoundIndex].clip;
                if (clip != null)
                {
                    soundEffectSource.PlayOneShot(clip);
                }
                context.nextReloadSoundIndex++;
            }
        }

        private static int FindLayerIndex(Animator animator, string layerName)
        {
            int layerIndex = animator.GetLayerIndex(layerName);
            if (layerIndex < 0)
            {
                Debug.LogError($"PlayerViewSystem >> 플레이어 Animator에 {layerName} 레이어가 없어 그 레이어의 가중치를 바꾸지 않는다.");
            }

            return layerIndex;
        }

        // 상태가 바뀔 때마다 새 상태에 맞는 파라미터를 건다. 새 행동에 들어갈 때만 트리거를 모두 지운다.
        // Idle과 Move에서 지우면 공격 상태가 바로 끝날 때 아직 소비되지 않은 공격 트리거가 사라진다.
        // Reload에서 Idle이나 Move로 돌아오면(장전 취소 포함) 원거리 레이어를 Shot Idle로 되돌린다.
        private static void ApplyStateRecords(in EntityIdentifier identifier, List<PlayerStateChangedRecord> stateRecords, Animator animator, int rangedLayerIndex)
        {
            for (int recordIndex = 0; recordIndex < stateRecords.Count; recordIndex++)
            {
                PlayerStateChangedRecord record = stateRecords[recordIndex];
                if (record.player.index != identifier.index || record.player.generation != identifier.generation)
                {
                    continue;
                }

                if (record.state != PlayerState.Idle && record.state != PlayerState.Move)
                {
                    for (int triggerIndex = 0; triggerIndex < TRIGGER_HASHES.Length; triggerIndex++)
                    {
                        animator.ResetTrigger(TRIGGER_HASHES[triggerIndex]);
                    }
                }

                switch (record.state)
                {
                    case PlayerState.Idle:
                        animator.SetFloat(MOVEMENT_SPEED_HASH, IDLE_SPEED);
                        break;
                    case PlayerState.Jump:
                        animator.SetBool(IS_JUMP_HASH, true);
                        break;
                    case PlayerState.Land:
                        if (record.previousState == PlayerState.Jump)
                        {
                            animator.SetBool(IS_JUMP_HASH, false);
                        }
                        break;
                    case PlayerState.Dodge:
                        animator.SetTrigger(DODGE_HASH);
                        break;
                    case PlayerState.Shot:
                        animator.SetTrigger(SHOT_HASH);
                        break;
                    case PlayerState.Swing:
                        animator.SetTrigger(SWING_HASH);
                        break;
                    case PlayerState.Swap:
                        animator.SetTrigger(SWAP_HASH);
                        break;
                    case PlayerState.Reload:
                        animator.SetTrigger(RELOAD_HASH);
                        break;
                    case PlayerState.Throw:
                        animator.SetTrigger(THROW_HASH);
                        break;
                    case PlayerState.Die:
                        animator.SetTrigger(DIE_HASH);
                        break;
                }

                if (record.previousState == PlayerState.Reload && (record.state == PlayerState.Idle || record.state == PlayerState.Move))
                {
                    ReturnRangedLayerFromReload(animator, rangedLayerIndex);
                }
            }
        }

        // 같은 프레임에 장전을 시작하고 취소하면 Reload 트리거가 소비되지 않고 남으므로 지운다.
        // 원거리 레이어가 아직 Reload에 있거나 Reload로 넘어가는 중일 때만 Shot Idle로 넘긴다. 이미 Shot Idle이면 다시 들어가지 않는다.
        private static void ReturnRangedLayerFromReload(Animator animator, int rangedLayerIndex)
        {
            animator.ResetTrigger(RELOAD_HASH);
            if (rangedLayerIndex < 0)
            {
                return;
            }

            bool isInReload = animator.GetCurrentAnimatorStateInfo(rangedLayerIndex).shortNameHash == RELOAD_STATE_HASH;
            bool isEnteringReload = animator.IsInTransition(rangedLayerIndex) && animator.GetNextAnimatorStateInfo(rangedLayerIndex).shortNameHash == RELOAD_STATE_HASH;
            if (isInReload || isEnteringReload)
            {
                animator.CrossFadeInFixedTime(SHOT_IDLE_STATE_HASH, RELOAD_INTERRUPT_CROSS_FADE_SECONDS, rangedLayerIndex);
            }
        }

        // 움직일 수 있는 상태(Move, Land, 공격, Swap, Reload)에서만 이동 입력과 달리기 여부로 MovementSpeed를 정한다. 다른 상태에서는 그대로 둔다.
        private static void ApplyMovementAnimation(in PlayerActionStateComponent actionState, in PlayerMotionComponent motion, Animator animator)
        {
            bool canMove = actionState.state == PlayerState.Move || actionState.state == PlayerState.Land || actionState.state == PlayerState.Shot || actionState.state == PlayerState.Swing || actionState.state == PlayerState.ThrowCharge || actionState.state == PlayerState.Throw || actionState.state == PlayerState.Swap || actionState.state == PlayerState.Reload;
            if (!canMove)
            {
                return;
            }

            float speed = IDLE_SPEED;
            if (motion.moveDirection != Vector3.zero)
            {
                speed = motion.isRunning ? RUN_SPEED : WALK_SPEED;
            }
            animator.SetFloat(MOVEMENT_SPEED_HASH, speed);
        }

        // 조준 자세는 직선 투사체 무기만 쓴다. 직선 투사체 무기가 아니거나 조준하지 않는 상태(Dodge, Jump, Land, Swap, Die)이면 원거리 레이어를 바로 끄고, 그 밖에는 rangedLayerBlendInTicks에 걸쳐 켠다.
        private static void ApplyRangedLayerWeight(in PlayerActionStateComponent actionState, in PlayerWeaponSlotsComponent weaponSlot, Entity[] weapons, Animator animator, int rangedLayerIndex, int rangedLayerBlendInTicks)
        {
            if (rangedLayerIndex < 0)
            {
                return;
            }

            bool isNotAimingState = actionState.state == PlayerState.Dodge || actionState.state == PlayerState.Jump || actionState.state == PlayerState.Land || actionState.state == PlayerState.Swap || actionState.state == PlayerState.Die;
            bool isHoldingRanged = weaponSlot.currentIndex < weaponSlot.weapons.Length && (weapons[weaponSlot.weapons[weaponSlot.currentIndex].index].mask & ComponentMask.StraightAttack) != 0;
            if (isNotAimingState || !isHoldingRanged)
            {
                animator.SetLayerWeight(rangedLayerIndex, LAYER_OFF_WEIGHT);
                return;
            }

            if (rangedLayerBlendInTicks <= 0)
            {
                animator.SetLayerWeight(rangedLayerIndex, LAYER_ON_WEIGHT);
                return;
            }

            float maxWeightDelta = Time.deltaTime / (rangedLayerBlendInTicks * Time.fixedDeltaTime);
            animator.SetLayerWeight(rangedLayerIndex, Mathf.MoveTowards(animator.GetLayerWeight(rangedLayerIndex), LAYER_ON_WEIGHT, maxWeightDelta));
        }

        // Swing, Throw, Swap은 행동 레이어에서 재생되므로 그 상태 동안만 켠다. 처음부터 보이도록 서서히 올리지 않는다.
        private static void ApplyActionLayerWeight(in PlayerActionStateComponent actionState, Animator animator, int actionLayerIndex)
        {
            if (actionLayerIndex < 0)
            {
                return;
            }

            bool isActionState = actionState.state == PlayerState.Swing || actionState.state == PlayerState.Throw || actionState.state == PlayerState.Swap;
            animator.SetLayerWeight(actionLayerIndex, isActionState ? LAYER_ON_WEIGHT : LAYER_OFF_WEIGHT);
        }

        private static void ApplyWeaponModels(in PlayerWeaponSlotsComponent weaponSlot, GameObject[] weaponModels)
        {
            for (int weaponIndex = 0; weaponIndex < weaponModels.Length; weaponIndex++)
            {
                GameObject model = weaponModels[weaponIndex];
                if (model == null)
                {
                    continue;
                }

                bool isCurrent = weaponIndex == weaponSlot.currentIndex;
                if (model.activeSelf != isCurrent)
                {
                    model.SetActive(isCurrent);
                }
            }
        }

        // 직전 틱과 이번 틱 사이를 지금 시각으로 보간한다.
        private static void ApplyTransform(in PositionComponent position, in RotationComponent rotation, Transform viewTransform)
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            viewTransform.SetPositionAndRotation(
                Vector3.Lerp(position.previous, position.current, alpha),
                Quaternion.Slerp(rotation.previous, rotation.current, alpha));
        }
    }
}
