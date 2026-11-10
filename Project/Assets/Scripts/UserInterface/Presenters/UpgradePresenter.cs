using TopView.Commands;
using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 강화 화면이 열려 있으면 UpgradeView를 보이고, 강화 버튼 입력을 강화 명령으로 넣는다.
    // 같은 값을 다시 그리지 않는 일은 항목(UpgradeEntry)이 맡는다.
    public class UpgradePresenter : MonoBehaviour
    {
        [SerializeField] private UpgradeView _view;

        private CommandBuffer _commandBuffer;
        private bool _isShown;

        /// <summary>
        /// 명령을 넣을 버퍼를 받고 View의 강화 항목을 만든 뒤 숨긴다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        /// <param name="commandBuffer">이번 판의 명령 버퍼</param>
        public void Initialize(CommandBuffer commandBuffer)
        {
            _commandBuffer = commandBuffer;
            _view.BuildEntries();
            _view.SetVisible(false);
            _isShown = false;
        }

        /// <summary>
        /// 보일지 말지가 바뀌었을 때만 View를 켜거나 끈다.
        /// </summary>
        /// <param name="isVisible">강화 화면을 보일지</param>
        public void Present(bool isVisible)
        {
            if (isVisible != _isShown)
            {
                _view.SetVisible(isVisible);
                _isShown = isVisible;
            }
        }

        /// <summary>
        /// Weapon이 아닌 항목 하나의 값을 보여 준다.
        /// </summary>
        /// <param name="type">강화 항목</param>
        /// <param name="level">현재 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <param name="cost">다음 레벨로 강화하는 비용</param>
        /// <param name="canUpgrade">버튼을 누를 수 있는지</param>
        public void PresentStat(PlayerUpgradeCommandType type, int level, int maxLevel, int cost, bool canUpgrade)
        {
            _view.SetStat(type, level, maxLevel, cost, canUpgrade);
        }

        /// <summary>
        /// 무기 하나의 강화 값을 보여 준다.
        /// </summary>
        /// <param name="weaponIndex">무기 번호</param>
        /// <param name="level">현재 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <param name="cost">다음 레벨로 강화하는 비용</param>
        /// <param name="canUpgrade">버튼을 누를 수 있는지</param>
        public void PresentWeapon(int weaponIndex, int level, int maxLevel, int cost, bool canUpgrade)
        {
            _view.SetWeapon(weaponIndex, level, maxLevel, cost, canUpgrade);
        }

        private void OnEnable()
        {
            _view.UpgradeClicked += HandleUpgradeClicked;
        }

        private void OnDisable()
        {
            _view.UpgradeClicked -= HandleUpgradeClicked;
        }

        private void HandleUpgradeClicked(PlayerUpgradeCommandType type, int weaponIndex)
        {
            if (_commandBuffer == null)
            {
                Debug.LogWarning($"UpgradePresenter >> 명령 버퍼가 연결되지 않아 {type} 강화 명령을 버렸다.");
                return;
            }

            _commandBuffer.playerUpgradeCommands.Add(new PlayerUpgradeCommand { type = type, weaponIndex = weaponIndex });
        }
    }
}
