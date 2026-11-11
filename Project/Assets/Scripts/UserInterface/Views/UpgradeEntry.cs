using System.Text;
using TMPro;
using TopView.Commands;
using TopView.Components;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 강화 항목 하나. 항목마다 프리팹을 따로 두고, 무엇을 강화하는지와 아이콘(Image)을 프리팹에 정해 둔다.
    // UpgradeView가 프리팹에서 만들어 소유하며, 레벨·비용·지금과 다음 레벨의 수치를 보여 주고 버튼 입력을 UpgradeView에 알린다.
    public class UpgradeEntry : MonoBehaviour
    {
        // 강화 항목
        [SerializeField] private PlayerUpgradeCommandType _type;
        // Weapon일 때 강화할 무기 번호. Player Definition의 무기 목록 번호(교체 키 - 1)다. 다른 항목은 쓰지 않는다
        [SerializeField] private int _weaponIndex;
        [FormerlySerializedAs("_text")]
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Button _button;
        // 설명에서 다음 레벨 수치에 입히는 색
        [SerializeField] private Color _nextValueColor = new Color(0.49f, 0.99f, 0f);

        private UpgradeView _owner;
        // 다음 레벨 수치 앞에 붙이는 리치 텍스트 색 태그. Initialize에서 _nextValueColor로 만든다
        private string _nextValueColorTag;
        private bool _hasPresented;
        private int _presentedLevel;
        private int _presentedMaxLevel;
        private int _presentedCost;
        private bool _presentedCanUpgrade;
        private float _presentedValue;
        private float _presentedNextValue;
        private WeaponUpgradeEffectComponent _presentedEffect;
        // 설명 글자를 만들 때 다시 쓰는 버퍼
        private readonly StringBuilder _descriptionBuilder = new StringBuilder();

        private const string MAX_LEVEL_COST_TEXT = "MAX";
        // 사용 중인 픽셀 폰트에 → 글자가 없어 영문 기호로 쓴다
        private const string ARROW = " > ";
        private const string MAX_HEALTH_LABEL = "최대 체력";
        private const string MAX_MANA_LABEL = "최대 마나";
        private const string RUN_MANA_COST_LABEL = "달리기 마나 소모(초당)";
        private const string DODGE_MANA_COST_LABEL = "구르기 마나 소모";
        private const string JUMP_MANA_COST_LABEL = "점프 마나 소모";
        private const string DAMAGE_LABEL = "피해량";
        private const string ATTACK_INTERVAL_LABEL = "공격 간격";
        private const string MAX_AMMO_LABEL = "탄창";
        private const string EXPLOSION_RADIUS_LABEL = "폭발 반경";
        private const string MELEE_RADIUS_LABEL = "근접 범위";
        private const string SECONDS_UNIT = "초";
        private const string METERS_UNIT = "m";
        private const string VALUE_FORMAT = "0.##";
        private const string COLOR_END_TAG = "</color>";
        private const string NO_BREAK_START_TAG = "<nobr>";
        private const string NO_BREAK_END_TAG = "</nobr>";
        // 설명에서 이름을 따로 한 줄에 두는 이름 길이(공백 포함). 이보다 짧은 이름은 수치와 한 줄에 붙인다
        private const int LABEL_OWN_LINE_LENGTH = 5;

        /// <summary>
        /// 강화 항목.
        /// </summary>
        public PlayerUpgradeCommandType Type => _type;

        /// <summary>
        /// Weapon일 때 강화할 무기 번호.
        /// </summary>
        public int WeaponIndex => _weaponIndex;

        /// <summary>
        /// 소유한 View를 정한다. UpgradeView가 항목을 만들 때 부른다.
        /// </summary>
        /// <param name="owner">이 항목을 소유한 View</param>
        public void Initialize(UpgradeView owner)
        {
            _owner = owner;
            _nextValueColorTag = $"<color=#{ColorUtility.ToHtmlStringRGB(_nextValueColor)}>";
            _hasPresented = false;
        }

        /// <summary>
        /// Weapon이 아닌 항목의 값이 바뀌었을 때만 레벨, 비용, 지금과 다음 레벨의 수치를 보여 준다.
        /// </summary>
        /// <param name="level">현재 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <param name="cost">다음 레벨로 강화하는 비용</param>
        /// <param name="canUpgrade">버튼을 누를 수 있는지</param>
        /// <param name="value">지금 레벨의 수치. 달리기 마나 소모는 틱당 값이다</param>
        /// <param name="nextValue">다음 레벨의 수치</param>
        public void SetStat(int level, int maxLevel, int cost, bool canUpgrade, float value, float nextValue)
        {
            if (IsSamePresented(level, maxLevel, cost, canUpgrade) && value == _presentedValue && nextValue == _presentedNextValue)
            {
                return;
            }

            SetLevelAndCost(level, maxLevel, cost, canUpgrade);
            _descriptionBuilder.Clear();
            bool isMaxLevel = level >= maxLevel;
            if (_type == PlayerUpgradeCommandType.RunManaCost)
            {
                // 틱당 값은 읽기 어려워 초당 값으로 보여 준다
                AppendLine(GetStatLabel(), value / Time.fixedDeltaTime, nextValue / Time.fixedDeltaTime, string.Empty, isMaxLevel);
            }
            else
            {
                AppendLine(GetStatLabel(), value, nextValue, string.Empty, isMaxLevel);
            }
            _descriptionText.text = _descriptionBuilder.ToString();

            _presentedValue = value;
            _presentedNextValue = nextValue;
        }

        /// <summary>
        /// Weapon 항목의 값이 바뀌었을 때만 레벨, 비용, 지금과 다음 레벨의 무기 수치를 보여 준다. 피해량은 항상, 나머지는 이 무기가 강화하는 효과만 보여 준다.
        /// </summary>
        /// <param name="level">현재 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <param name="cost">다음 레벨로 강화하는 비용</param>
        /// <param name="canUpgrade">버튼을 누를 수 있는지</param>
        /// <param name="effect">지금과 다음 레벨의 무기 수치</param>
        public void SetWeapon(int level, int maxLevel, int cost, bool canUpgrade, in WeaponUpgradeEffectComponent effect)
        {
            if (IsSamePresented(level, maxLevel, cost, canUpgrade) && IsSameEffect(in effect, in _presentedEffect))
            {
                return;
            }

            SetLevelAndCost(level, maxLevel, cost, canUpgrade);
            _descriptionBuilder.Clear();
            bool isMaxLevel = level >= maxLevel;
            AppendLine(DAMAGE_LABEL, effect.currentDamage, effect.nextDamage, string.Empty, isMaxLevel);
            if (effect.currentAttackIntervalTicks != effect.nextAttackIntervalTicks)
            {
                AppendLine(ATTACK_INTERVAL_LABEL, effect.currentAttackIntervalTicks * Time.fixedDeltaTime, effect.nextAttackIntervalTicks * Time.fixedDeltaTime, SECONDS_UNIT, isMaxLevel);
            }
            if (effect.currentMaxAmmo != effect.nextMaxAmmo)
            {
                AppendLine(MAX_AMMO_LABEL, effect.currentMaxAmmo, effect.nextMaxAmmo, string.Empty, isMaxLevel);
            }
            if (effect.currentExplosionRadius != effect.nextExplosionRadius)
            {
                AppendLine(EXPLOSION_RADIUS_LABEL, effect.currentExplosionRadius, effect.nextExplosionRadius, METERS_UNIT, isMaxLevel);
            }
            if (effect.currentMeleeRadius != effect.nextMeleeRadius)
            {
                AppendLine(MELEE_RADIUS_LABEL, effect.currentMeleeRadius, effect.nextMeleeRadius, METERS_UNIT, isMaxLevel);
            }
            _descriptionText.text = _descriptionBuilder.ToString();

            _presentedEffect = effect;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleButtonClicked);
        }

        private bool IsSamePresented(int level, int maxLevel, int cost, bool canUpgrade)
        {
            return _hasPresented && level == _presentedLevel && maxLevel == _presentedMaxLevel && cost == _presentedCost && canUpgrade == _presentedCanUpgrade;
        }

        // 최대 레벨이면 비용 대신 MAX를 보여 준다
        private void SetLevelAndCost(int level, int maxLevel, int cost, bool canUpgrade)
        {
            _levelText.text = $"Lv {level}/{maxLevel}";
            _costText.text = level >= maxLevel ? MAX_LEVEL_COST_TEXT : $"{cost:N0} G";
            _button.interactable = canUpgrade;
            _presentedLevel = level;
            _presentedMaxLevel = maxLevel;
            _presentedCost = cost;
            _presentedCanUpgrade = canUpgrade;
            _hasPresented = true;
        }

        // 효과 하나를 붙이고, 다음 수치(단위 포함)에만 색을 입힌다. 최대 레벨이면 다음 수치 없이 지금 수치만 붙인다.
        // 이름이 LABEL_OWN_LINE_LENGTH자(공백 포함) 이상이면 "이름" 줄과 "지금 > 다음" 줄로 나누고,
        // 짧으면 "이름 지금 > 다음"을 한 줄로 붙여 nobr로 감싼다. nobr 안에서는 자동 줄바꿈이 일어나지 않는다
        private void AppendLine(string label, float value, float nextValue, string unit, bool isMaxLevel)
        {
            bool isLabelOnOwnLine = label.Length >= LABEL_OWN_LINE_LENGTH;
            if (_descriptionBuilder.Length > 0)
            {
                _descriptionBuilder.Append('\n');
            }
            if (isLabelOnOwnLine)
            {
                _descriptionBuilder.Append(label).Append('\n');
            }
            else
            {
                _descriptionBuilder.Append(NO_BREAK_START_TAG).Append(label).Append(' ');
            }
            _descriptionBuilder.Append(value.ToString(VALUE_FORMAT)).Append(unit);
            if (!isMaxLevel)
            {
                _descriptionBuilder.Append(ARROW).Append(_nextValueColorTag).Append(nextValue.ToString(VALUE_FORMAT)).Append(unit).Append(COLOR_END_TAG);
            }
            if (!isLabelOnOwnLine)
            {
                _descriptionBuilder.Append(NO_BREAK_END_TAG);
            }
        }

        private string GetStatLabel()
        {
            switch (_type)
            {
                case PlayerUpgradeCommandType.MaxHealth:
                    return MAX_HEALTH_LABEL;
                case PlayerUpgradeCommandType.MaxMana:
                    return MAX_MANA_LABEL;
                case PlayerUpgradeCommandType.RunManaCost:
                    return RUN_MANA_COST_LABEL;
                case PlayerUpgradeCommandType.DodgeManaCost:
                    return DODGE_MANA_COST_LABEL;
                case PlayerUpgradeCommandType.JumpManaCost:
                    return JUMP_MANA_COST_LABEL;
                default:
                    // 설명은 값이 바뀔 때만 다시 만들므로 이 경고는 매 프레임 쌓이지 않는다
                    Debug.LogWarning($"UpgradeEntry >> {_type}의 이름표가 없어 항목 이름을 그대로 보여 준다. GetStatLabel에 case를 추가한다.");
                    return _type.ToString();
            }
        }

        private static bool IsSameEffect(in WeaponUpgradeEffectComponent effect, in WeaponUpgradeEffectComponent other)
        {
            return effect.currentDamage == other.currentDamage && effect.nextDamage == other.nextDamage
                && effect.currentAttackIntervalTicks == other.currentAttackIntervalTicks && effect.nextAttackIntervalTicks == other.nextAttackIntervalTicks
                && effect.currentMaxAmmo == other.currentMaxAmmo && effect.nextMaxAmmo == other.nextMaxAmmo
                && effect.currentExplosionRadius == other.currentExplosionRadius && effect.nextExplosionRadius == other.nextExplosionRadius
                && effect.currentMeleeRadius == other.currentMeleeRadius && effect.nextMeleeRadius == other.nextMeleeRadius;
        }

        private void HandleButtonClicked()
        {
            if (_owner == null)
            {
                Debug.LogWarning($"UpgradeEntry >> 소유한 UpgradeView가 정해지지 않아 {_type} 항목의 버튼 입력을 버렸다.");
                return;
            }

            _owner.ClickEntry(_type, _weaponIndex);
        }
    }
}
