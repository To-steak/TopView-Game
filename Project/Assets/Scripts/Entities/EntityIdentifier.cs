namespace TopView.Entities
{
    // 엔티티를 가리키는 값. 같은 칸이 재사용되어도 generation으로 이미 사라진 엔티티인지 가린다.
    public struct EntityIdentifier
    {
        // Storage 배열의 칸 번호
        public int index;
        // 그 칸이 채워진 횟수. 칸의 generation과 다르면 이미 사라진 엔티티다
        public int generation;
    }
}
