namespace TopView.Components
{
    // 플레이어 감지기의 범위 모양. 감지 판정 방식을 정한다
    public enum PlayerDetectorShapeKind
    {
        // XZ 평면의 원. detectionRadius를 쓴다
        Circle,
        // XZ 평면의 월드 축에 맞춘 사각형. 회전하지 않으며 detectionSize를 쓴다
        Rectangle
    }
}
