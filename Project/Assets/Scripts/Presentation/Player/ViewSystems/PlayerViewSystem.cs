using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Presentation.Player.Views;
using TopView.Records;
using UnityEngine;

namespace TopView.Presentation.Player.ViewSystems
{
    // 플레이어 View를 만들고, 위치·회전 보간, Animator 파라미터, 무기 모델을 엔티티에 맞춘다.
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

        // MovementSpeed 파라미터 값
        private const float IDLE_SPEED = 0f;
        private const float WALK_SPEED = 1f;
        private const float RUN_SPEED = 2f;

        private const string UPPER_LAYER_NAME = "Upper Layer";
        private const float UPPER_LAYER_ON_WEIGHT = 1f;
        private const float UPPER_LAYER_OFF_WEIGHT = 0f;

        /// <summary>
        /// 살아 있는 플레이어가 있으면 View를 만들거나 맞추고, 없으면 View를 끈다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="stateRecords">이번 프레임의 상태 변경 기록</param>
        /// <param name="viewPrefab">PlayerView가 붙은 프리팹</param>
        /// <param name="weaponModelPrefabs">무기 번호 순서의 무기 모델 프리팹. 없는 칸은 null</param>
        /// <param name="upperLayerBlendInTicks">상체 레이어 가중치가 0에서 1로 올라오는 시간(틱)</param>
        /// <param name="context">View 실행 상태</param>
        public static void Present(PlayerStorage players, List<PlayerStateChangedRecord> stateRecords, GameObject viewPrefab, GameObject[] weaponModelPrefabs, int upperLayerBlendInTicks, ViewContext context)
        {
            if (!TryFindAlivePlayer(players, out int playerIndex))
            {
                if (context.playerView != null && context.playerView.gameObject.activeSelf)
                {
                    context.playerView.gameObject.SetActive(false);
                }
                return;
            }

            ref readonly PlayerEntity player = ref players.entities[playerIndex];
            EntityIdentifier identifier = new EntityIdentifier { index = playerIndex, generation = player.generation };

            if (context.playerView == null)
            {
                CreateView(in player, viewPrefab, weaponModelPrefabs, context);
            }
            context.playerViewEntity = identifier;
            if (!context.playerView.gameObject.activeSelf)
            {
                context.playerView.gameObject.SetActive(true);
            }

            Animator animator = context.playerView.animator;
            ApplyStateRecords(in identifier, stateRecords, animator);
            ApplyMovementAnimation(in player, animator);
            ApplyUpperLayerWeight(in player, animator, context.playerUpperLayerIndex, upperLayerBlendInTicks);
            ApplyWeaponModels(in player, context.playerWeaponModels);
            ApplyTransform(in player, context.playerView.transform);
        }

        private static bool TryFindAlivePlayer(PlayerStorage players, out int playerIndex)
        {
            for (playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                if (players.entities[playerIndex].isAlive)
                {
                    return true;
                }
            }

            playerIndex = -1;
            return false;
        }

        private static void CreateView(in PlayerEntity player, GameObject viewPrefab, GameObject[] weaponModelPrefabs, ViewContext context)
        {
            GameObject viewObject = Object.Instantiate(viewPrefab, player.currentPosition, player.currentRotation);
            context.playerView = viewObject.GetComponent<PlayerView>();
            context.playerUpperLayerIndex = context.playerView.animator.GetLayerIndex(UPPER_LAYER_NAME);
            if (context.playerUpperLayerIndex < 0)
            {
                Debug.LogError($"PlayerViewSystem >> 플레이어 Animator에 {UPPER_LAYER_NAME} 레이어가 없어 상체 레이어 가중치를 바꾸지 않는다.");
            }

            context.playerWeaponModels = new GameObject[weaponModelPrefabs.Length];
            for (int weaponIndex = 0; weaponIndex < weaponModelPrefabs.Length; weaponIndex++)
            {
                if (weaponModelPrefabs[weaponIndex] == null)
                {
                    continue;
                }

                GameObject model = Object.Instantiate(weaponModelPrefabs[weaponIndex], context.playerView.weaponSocket);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.SetActive(false);
                context.playerWeaponModels[weaponIndex] = model;
            }
        }

        // 상태가 바뀔 때마다 새 상태에 맞는 파라미터를 건다. 새 행동에 들어갈 때만 트리거를 모두 지운다.
        // Idle과 Move에서 지우면 공격 상태가 바로 끝날 때 아직 소비되지 않은 공격 트리거가 사라진다.
        private static void ApplyStateRecords(in EntityIdentifier identifier, List<PlayerStateChangedRecord> stateRecords, Animator animator)
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
            }
        }

        // Move, Land, Reload 상태에서만 이동 입력과 달리기 여부로 MovementSpeed를 정한다. 다른 상태에서는 그대로 둔다.
        // Reload는 상체 레이어에서 재생되므로, 멈춰 있는 동안 하체가 Idle을 재생하도록 맞춘다.
        private static void ApplyMovementAnimation(in PlayerEntity player, Animator animator)
        {
            if (player.state != PlayerState.Move && player.state != PlayerState.Land && player.state != PlayerState.Reload)
            {
                return;
            }

            float speed = IDLE_SPEED;
            if (player.moveDirection != Vector3.zero)
            {
                speed = player.isRunning ? RUN_SPEED : WALK_SPEED;
            }
            animator.SetFloat(MOVEMENT_SPEED_HASH, speed);
        }

        // 조준 자세는 Ranged만 쓴다. Ranged가 아니거나 전신 모션(Dodge, Jump, Land, Swap, Die)이면 상체 레이어를 바로 끄고, 그 밖에는 upperLayerBlendInTicks에 걸쳐 켠다.
        private static void ApplyUpperLayerWeight(in PlayerEntity player, Animator animator, int upperLayerIndex, int upperLayerBlendInTicks)
        {
            if (upperLayerIndex < 0)
            {
                return;
            }

            bool isFullBodyState = player.state == PlayerState.Dodge || player.state == PlayerState.Jump || player.state == PlayerState.Land || player.state == PlayerState.Swap || player.state == PlayerState.Die;
            bool isHoldingRanged = player.currentWeaponIndex < player.weapons.Length && player.weapons[player.currentWeaponIndex].kind == WeaponKind.Ranged;
            if (isFullBodyState || !isHoldingRanged)
            {
                animator.SetLayerWeight(upperLayerIndex, UPPER_LAYER_OFF_WEIGHT);
                return;
            }

            if (upperLayerBlendInTicks <= 0)
            {
                animator.SetLayerWeight(upperLayerIndex, UPPER_LAYER_ON_WEIGHT);
                return;
            }

            float maxWeightDelta = Time.deltaTime / (upperLayerBlendInTicks * Time.fixedDeltaTime);
            animator.SetLayerWeight(upperLayerIndex, Mathf.MoveTowards(animator.GetLayerWeight(upperLayerIndex), UPPER_LAYER_ON_WEIGHT, maxWeightDelta));
        }

        private static void ApplyWeaponModels(in PlayerEntity player, GameObject[] weaponModels)
        {
            for (int weaponIndex = 0; weaponIndex < weaponModels.Length; weaponIndex++)
            {
                GameObject model = weaponModels[weaponIndex];
                if (model == null)
                {
                    continue;
                }

                bool isCurrent = weaponIndex == player.currentWeaponIndex;
                if (model.activeSelf != isCurrent)
                {
                    model.SetActive(isCurrent);
                }
            }
        }

        // 직전 틱과 이번 틱 사이를 지금 시각으로 보간한다.
        private static void ApplyTransform(in PlayerEntity player, Transform viewTransform)
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            viewTransform.SetPositionAndRotation(
                Vector3.Lerp(player.previousPosition, player.currentPosition, alpha),
                Quaternion.Slerp(player.previousRotation, player.currentRotation, alpha));
        }
    }
}
