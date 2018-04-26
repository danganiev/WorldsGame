using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Saving.DataClasses.Items;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI.ModelEditor
{
    internal class AddEffectPanel : View.GUI.GUI
    {
        private const int PANEL_WIDTH = 500;
        private const int PANEL_HEIGHT = 650;

        private const int LABEL_MARGIN = 50;

        private const int LIST_WIDTH = 250;
        private const int LIST_HEIGHT = 70;

        private readonly View.GUI.GUI _parentGUI;
        private readonly WorldSettings _worldSettings;
        private LabelControl _titleLabel;
        private EffectType? _currentlySelectedEffectType;
        private EffectTarget _currentlySelectedTarget;
        private Dictionary<EffectType?, List<Control>> _controlsByEffectType;
        private NumberInputControl _damageInput;

        private ListControl _effectsTypeList;
        private ListControl _blockList;
        private ListControl _effectsTargetList;        
        private LabelControl _effectsTypeLabel;

        private ListControl _attributesList;
        private LabelControl _attributesLabel;
        private LabelControl _attributeValueLabel;
        private NumberInputControl _attributeValueInput;

        private List<EffectType> _currentEffects;
        private NumberInputControl _areaInput;

        internal int ActionNumber { get; set; }

        internal PanelControl EffectPanel { get; private set; }

        internal event Action OnGoingBack = () => { };

        internal event Action<Effect, int> OnAddition = (effect, actionNumber) => { };

        internal AddEffectPanel(View.GUI.GUI parentGUI, WorldSettings worldSettings)
        {
            _parentGUI = parentGUI;
            _worldSettings = worldSettings;
            EffectPanel = new PanelControl();
            _controlsByEffectType = new Dictionary<EffectType?, List<Control>>();
            _currentEffects = new List<EffectType>();
            _currentlySelectedEffectType = null;

            foreach (EffectType et in EnumUtils.GetValues<EffectType>())
            {
                _controlsByEffectType.Add(et, new List<Control>());
            }
            //            Initialize();
        }

        protected override void CreateControls()
        {
            AddMainPanel();

            AddEffectsAreaList();

            AddEffectsTargetList();
            AddEffectsTypeList();
            AddAttributesList();

            AddBlockControls();

            _effectsTargetList.SelectedItems.Add(0);

            ReloadEffectsList(EffectTarget.Block);
        }

        protected override void LoadData()
        {
            //            LoadList(_effectsTypeList, EffectNaming.GetEffectNames().Values);
            LoadList(_effectsTargetList, EffectNaming.GetEffectTargetNames().Values);            
            LoadList(_attributesList, CharacterAttribute.SaverHelper(_worldSettings.Name));
        }

        private void AddMainPanel()
        {
            const int Y = 15;
            const int X = 30;

            var panelStartX = (int)_parentGUI.Screen.Width / 2 - PANEL_WIDTH / 2;
            var panelStartY = (int)_parentGUI.Screen.Height / 2 - PANEL_HEIGHT / 2;

            EffectPanel = new PanelControl
            {
                Bounds = new UniRectangle(panelStartX, panelStartY, PANEL_WIDTH, PANEL_HEIGHT)
            };

            _titleLabel = new LabelControl
            {
                Text = "Add effect",
                Bounds = new UniRectangle(X, Y, 130, _parentGUI.LabelHeight),
                IsHeader = true
            };

            var backButton = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(new UniScalar(1f, -(X + 70)), new UniScalar(1f, -(X + 30)), 70, 30)
            };
            backButton.Pressed += (sender, args) => Back();

            var addButton = new ButtonControl
            {
                Text = "Add",
                Bounds = new UniRectangle(new UniScalar(1f, -(X + 150)), new UniScalar(1f, -(X + 30)), 70, 30)
            };
            addButton.Pressed += (sender, args) => Add();

            EffectPanel.Children.Add(_titleLabel);
            EffectPanel.Children.Add(backButton);
            EffectPanel.Children.Add(addButton);

            EffectPanel.BringToFront();
        }

        private void AddEffectsAreaList()
        {
            var Y = _titleLabel.Bounds.Bottom + 50;

            var label = new LabelControl
            {
                Bounds = new UniRectangle(_titleLabel.Bounds.Left, Y, 80, 20),
                Text = "Effect area radius:",
                PopupText = "(0-32)"
            };

            _areaInput = new NumberInputControl
            {
                Bounds = new UniRectangle(label.Bounds.Right + LABEL_MARGIN, label.Bounds.Top - 5, 70, ButtonHeight),
                MaxValue = 32,
                MinValue = 0,
                Text = "0",
                IsPositiveOnly = false
            };

            EffectPanel.Children.Add(label);
            EffectPanel.Children.Add(_areaInput);
        }

        private void AddEffectsTargetList()
        {
            var Y = _areaInput.Bounds.Bottom + 10;

            var label = new LabelControl
            {
                Bounds = new UniRectangle(_titleLabel.Bounds.Left, Y, 80, 20),
                Text = "Effect targets:"
            };

            _effectsTargetList = new ListControl
            {
                Bounds = new UniRectangle(label.Bounds.Right + LABEL_MARGIN, Y, LIST_WIDTH, LIST_HEIGHT),
                SelectionMode = ListSelectionMode.Single
            };
            _effectsTargetList.SelectionChanged += OnTargetSelected;

            EffectPanel.Children.Add(label);
            EffectPanel.Children.Add(_effectsTargetList);
        }

        private void OnTargetSelected(object sender, EventArgs eventArgs)
        {
            EffectTarget selectedTarget = (EffectTarget)_effectsTargetList.SelectedItems[0];

            if (selectedTarget != _currentlySelectedTarget)
            {
                ReloadEffectsList(selectedTarget);
                _currentlySelectedTarget = selectedTarget;
            }

            if (!EffectPanel.Children.Contains(_effectsTypeList))
            {
                EffectPanel.Children.Add(_effectsTypeLabel);
                EffectPanel.Children.Add(_effectsTypeList);
            }
        }

        private void AddEffectsTypeList()
        {
            var Y = _effectsTargetList.Bounds.Bottom + 10;

            _effectsTypeLabel = new LabelControl
                                    {
                                        Bounds = new UniRectangle(_titleLabel.Bounds.Left, Y, 80, 20),
                                        Text = "Effect types:"
                                    };

            _effectsTypeList = new ListControl
            {
                Bounds = new UniRectangle(_effectsTypeLabel.Bounds.Right + LABEL_MARGIN, Y, LIST_WIDTH, LIST_HEIGHT),
                SelectionMode = ListSelectionMode.Single
            };
            _effectsTypeList.SelectionChanged += OnEffectTypeSelected;
        }

        private void AddAttributesList()
        {
            var Y = _effectsTypeList.Bounds.Bottom + 10;

            _attributesLabel = new LabelControl
            {
                Bounds = new UniRectangle(_titleLabel.Bounds.Left, Y, 80, 20),
                Text = "Attribute:"
            };

            _attributesList = new ListControl
            {
                Bounds = new UniRectangle(_attributesLabel.Bounds.Right + LABEL_MARGIN, Y, LIST_WIDTH, LIST_HEIGHT),
                SelectionMode = ListSelectionMode.Single
            };
            //            _attributesList.SelectionChanged += OnEffectTypeSelected;

            _attributeValueLabel = new LabelControl
            {
                Bounds = new UniRectangle(_titleLabel.Bounds.Left, _attributesList.Bounds.Bottom + 10, 80, 20),
                Text = "Value:",
                PopupText = "You can use negative values"
            };

            _attributeValueInput = new NumberInputControl
            {
                Bounds = new UniRectangle(_attributeValueLabel.Bounds.Right + LABEL_MARGIN, _attributesList.Bounds.Bottom + 10, 70, ButtonHeight),
                MaxValue = 10000,
                MinValue = -10000,
                IsPositiveOnly = false
            };

            _controlsByEffectType[EffectType.ChangeAttribute].Add(_attributesLabel);
            _controlsByEffectType[EffectType.ChangeAttribute].Add(_attributesList);

            _controlsByEffectType[EffectType.ChangeAttribute].Add(_attributeValueLabel);
            _controlsByEffectType[EffectType.ChangeAttribute].Add(_attributeValueInput);
        }

        private void OnEffectTypeSelected(object sender, EventArgs eventArgs)
        {
            if (_effectsTypeList.SelectedItems.Count > 0)
            {
                var selectedEffectType = _currentEffects[_effectsTypeList.SelectedItems[0]];

                if (selectedEffectType != _currentlySelectedEffectType)
                {
                    ChangeEffectTypeControls(selectedEffectType);
                    _currentlySelectedEffectType = selectedEffectType;
                }
            }
            else
            {
                ClearAllEffectSpecificControls();
            }
        }

        private void ReloadEffectsList(EffectTarget selectedTarget)
        {
            Dictionary<int, string> effects = EffectNaming.GetEffectNames();

            var effectNames = new List<String>();
            _currentEffects.Clear();
            _effectsTypeList.SelectedItems.Clear();
            ClearAllEffectSpecificControls();

            if (selectedTarget == EffectTarget.Block)
            {
                foreach (PossibleBlockEffect effect in EnumUtils.GetValues<PossibleBlockEffect>())
                {
                    effectNames.Add(effects[(int)effect]);
                    _currentEffects.Add((EffectType)effect);
                }
            }
            else if (selectedTarget == EffectTarget.Self || selectedTarget == EffectTarget.Creature)
            {
                foreach (PossibleCreatureEffect effect in EnumUtils.GetValues<PossibleCreatureEffect>())
                {
                    effectNames.Add(effects[(int)effect]);
                    _currentEffects.Add((EffectType)effect);
                }
            }

            LoadList(_effectsTypeList, effectNames);
        }

        private void ChangeEffectTypeControls(EffectType selectedType)
        {
            if (_currentlySelectedEffectType != null)
            {
                foreach (Control control in _controlsByEffectType[_currentlySelectedEffectType])
                {
                    EffectPanel.Children.Remove(control);
                }
            }

            foreach (Control control in _controlsByEffectType[selectedType])
            {
                EffectPanel.Children.Add(control);
            }
        }

        private void ClearAllEffectSpecificControls()
        {
            _currentlySelectedEffectType = null;

            foreach (EffectType effectType in EnumUtils.GetValues<EffectType>())
            {
                foreach (Control control in _controlsByEffectType[effectType])
                {
                    EffectPanel.Children.Remove(control);
                }
            }
        }

        private void AddBlockControls()
        {
            var Y = _effectsTypeList.Bounds.Bottom + 10;

            var damageLabel = new LabelControl
            {
                Bounds = new UniRectangle(_titleLabel.Bounds.Left, Y, 80, 20),
                Text = "Damage:",
                // PopupText = "Always applies to selected block"
            };

            _damageInput = new NumberInputControl
            {
                Bounds = new UniRectangle(damageLabel.Bounds.Right + LABEL_MARGIN, Y, 70, ButtonHeight),
                MaxValue = 10000,
                MinValue = 0,
                IsPositiveOnly = true
            };

            var blocksLabel = new LabelControl
            {
                Bounds = new UniRectangle(_titleLabel.Bounds.Left, _damageInput.Bounds.Bottom + 10, 80, 20),
                Text = "Blocks:"
            };

            _blockList = new ListControl
            {
                Bounds = new UniRectangle(blocksLabel.Bounds.Right + LABEL_MARGIN, _damageInput.Bounds.Bottom + 10, LIST_WIDTH, ListHeight),
                SelectionMode = ListSelectionMode.Multi
            };

            LoadList(_blockList, Block.SaverHelper(_worldSettings.Name));

            _controlsByEffectType[EffectType.DamageBlock].Add(damageLabel);
            _controlsByEffectType[EffectType.DamageBlock].Add(_damageInput);
            _controlsByEffectType[EffectType.DamageBlock].Add(blocksLabel);
            _controlsByEffectType[EffectType.DamageBlock].Add(_blockList);

            _controlsByEffectType[EffectType.DamageAllBlocksExcept].Add(damageLabel);
            _controlsByEffectType[EffectType.DamageAllBlocksExcept].Add(_damageInput);
            _controlsByEffectType[EffectType.DamageAllBlocksExcept].Add(blocksLabel);
            _controlsByEffectType[EffectType.DamageAllBlocksExcept].Add(_blockList);
        }

        protected override void Back()
        {
            OnGoingBack();
        }

        private void Add()
        {
            var returnEffect = new Effect
            {
                EffectTarget = _currentlySelectedTarget,
                EffectArea = 0
            };

            if (_currentlySelectedEffectType != null)
            {
                returnEffect.EffectType = (EffectType)_currentlySelectedEffectType;
            }
            else
            {
                _parentGUI.ShowAlertBox("Effect must have a target");
                return;
            }

            returnEffect.EffectType = (EffectType)_currentlySelectedEffectType;
            returnEffect.EffectArea = _areaInput.GetInt();

            switch (_currentlySelectedEffectType)
            {
                case EffectType.DamageAllBlocksExcept:
                case EffectType.DamageBlock:
                    break;

                case EffectType.ChangeAttribute:
                    if (!_attributesList.IsSelected())
                    {
                        _parentGUI.ShowAlertBox("Please select an attribute");
                        return;
                    }
                    returnEffect.ApplicantName = _attributesList.SelectedName();
                    returnEffect.Value = _attributeValueInput.GetFloat();
                    break;
            }

            OnAddition(returnEffect, ActionNumber);
            Back();
        }
    }
}