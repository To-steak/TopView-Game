namespace TopView.Editor
{
    // Definition 편집 창의 탭 안에서 목록을 나누는 묶음 하나
    public struct DefinitionEditorGroup
    {
        // 목록에 그리는 머리글. null이면 머리글 없이 에셋만 그린다
        public string label;
        // AssetDatabase.FindAssets에 넘기는 검색 필터. 예: "t:WeaponDefinition"
        public string filter;
    }
}
