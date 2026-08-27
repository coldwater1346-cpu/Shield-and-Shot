using Shield_Shot.GameplayCore.Weapon.Projectile;


    [System.Serializable]
    public struct ActiveBehavior
    {
        public ProjectileBehaviorSO BehaviorSO; // 등록된 특성 데이터 공장
        public int Level;                       // 현재 중첩된 레벨

        public ActiveBehavior(ProjectileBehaviorSO so, int level)
        {
            BehaviorSO = so;
            Level = level;
        }
    }