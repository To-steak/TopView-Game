using System;
using UnityEngine;

namespace TopView.Components
{
    // 플레이어 감지기의 범위 설정값. Player Detector Authoring에서 정한다.
    [Serializable]
    public struct PlayerDetectorRangeComponent
    {
        // 범위 모양
        public PlayerDetectorShapeKind shape;
        // 감지 반경(유닛). 플레이어 캡슐 가장자리가 이 원에 닿으면 감지한다. Circle만 쓴다
        public float detectionRadius;
        // 감지 사각형의 전체 크기(유닛). x는 월드 X, y는 월드 Z 길이다. 플레이어 캡슐 가장자리가 이 사각형에 닿으면 감지한다. Rectangle만 쓴다
        public Vector2 detectionSize;
    }
}
