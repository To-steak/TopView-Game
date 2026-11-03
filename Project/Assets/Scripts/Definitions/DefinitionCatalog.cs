using UnityEngine;

namespace TopView.Definitions
{
    // Authoring이 가리키지 않는 Definition과 프리팹을 모은다. Runner가 시작할 때 Resources에서 이 에셋 하나만 읽는다.
    [CreateAssetMenu(fileName = "Definition Catalog", menuName = "Definitions/Definition Catalog")]
    public class DefinitionCatalog : ScriptableObject
    {
        // 한 판의 규칙과 저장소 용량
        public GameSettingsDefinition gameSettings;
        // 전리품의 규칙과 종류별 프리팹
        public LootDefinition loot;
        // WorldTextView가 붙은 월드 글자 프리팹
        public GameObject worldTextPrefab;
    }
}
