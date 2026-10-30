using UnityEngine;

namespace TopView.Definitions
{
    // Authoring이 가리키지 않는 Definition을 모은다. Runner가 시작할 때 Resources에서 이 에셋 하나만 읽는다.
    [CreateAssetMenu(fileName = "Definition Catalog", menuName = "Definitions/Definition Catalog")]
    public class DefinitionCatalog : ScriptableObject
    {
        // 한 판의 규칙과 웨이브 구성
        public GameSettingsDefinition gameSettings;
        // 플레이어의 값과 무기 구성
        public PlayerDefinition player;
    }
}
