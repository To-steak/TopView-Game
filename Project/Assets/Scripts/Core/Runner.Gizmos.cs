#if UNITY_EDITOR
namespace TopView.Core
{
    // 디버그 표시: 요소별 Gizmo를 부른다. 그리는 방법은 각 요소의 Runner.<요소>.cs에 둔다
    public partial class Runner
    {
        private void OnDrawGizmos()
        {
            if (_world == null)
            {
                return;
            }

            DrawPlayerGizmos();
        }
    }
}
#endif
