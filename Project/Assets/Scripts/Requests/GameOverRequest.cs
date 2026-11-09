namespace TopView.Requests
{
    // 판을 끝내 달라는 요청. 플레이어 사망은 PlayerStateSystem이 패배로, 게임을 끝내는 적의 사망은 EnemyDespawnSystem이 승리로 넣는다.
    public struct GameOverRequest
    {
        // 승리로 끝내는지. false면 패배다
        public bool isVictory;
    }
}
