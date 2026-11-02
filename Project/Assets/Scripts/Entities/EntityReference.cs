namespace TopView.Entities
{
    // 저장소 종류까지 함께 담아 엔티티를 가리키는 값. 여러 종류의 엔티티를 한 목록이나 큐에서 가리킬 때 쓴다.
    public struct EntityReference
    {
        // 엔티티가 담긴 저장소의 종류
        public EntityKind kind;
        // 그 저장소 안에서 엔티티를 가리키는 값
        public EntityIdentifier identifier;
    }
}
