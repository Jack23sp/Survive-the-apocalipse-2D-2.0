using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public partial class Skills
{
    [Command]
    public void CmdResetSkill()
    {
        CancelCast(true);
        GetComponent<Player>()._state = "IDLE";
    }
}

public class UIMobileControl : MonoBehaviour
{
    public static UIMobileControl singleton;
    public GameObject panel;
    public Transform[] slots;
    public VariableJoystick joystick;
    public Image weaponImage;
    public GameObject durabilityGameobject;
    public Image durabilitySlider;
    public Button chargeButton;
    public Image munitionImage;
    public TextMeshProUGUI munitionCountText;
    public Button enableAttackButton;
    public GameObject enableAttackButtonObject;
    public GameObject placeOver;
    public Button runButton;
    public Button sneakButton;

    public Item munition;
    public ScaleAnimation scaleAnimation;
    public bool animate = false;

    private Player player;
    //[HideInInspector] public int munitionCount;
    [HideInInspector] public int prevMunitionCount;

    [HideInInspector] public int durability;
    [HideInInspector] public int prevDurability;

    [HideInInspector] public bool hasWeapon;

    [HideInInspector] public int prevWeaponMunitionCount;

    public void Awake()
    {
        
        if (!singleton) singleton = this;

        chargeButton.onClick.RemoveAllListeners();
        chargeButton.onClick.AddListener(() =>
        {
            // Cmd Charge
            if (player.playerEquipment.slots[0].amount > 0)
                player.playerWeapon.CmdChargeMunition(player.playerEquipment.slots[0].item.name);
        });

        runButton.onClick.RemoveAllListeners();
        runButton.onClick.AddListener(() =>
        {
            player.playerMove.SetRun();
        });

        sneakButton.onClick.RemoveAllListeners();
        sneakButton.onClick.AddListener(() =>
        {

            player.playerMove.SetSneak();
        });

        enableAttackButton.onClick.RemoveAllListeners();
        enableAttackButton.onClick.AddListener(() =>
        {
            player.playerMove.CmdSetCanAttack(!player.playerMove.canAttack);
        });
    }

    public void Update()
    {
        if (!player) player = Player.localPlayer;
        if (player != null)
        {
            panel.SetActive(true);
            player.playerNavMeshMovement2D.MoveJoystick();

            if (!AttackManager.singleton.overrideControls)
            {
                
                if (AttackManager.singleton && !AttackManager.singleton.isPC) 
                    player.playerNavMeshMovement2D.JoystickHandling();

                if (AttackManager.singleton && AttackManager.singleton.isPC)
                {
                    player.playerNavMeshMovement2D.joystick.input = Vector2.zero;
                    player.playerNavMeshMovement2D.RotateByclick();
                }
            }
            else
            {
                player.playerNavMeshMovement2D.MoveJoystick();
                player.playerNavMeshMovement2D.RotateByclick();
            }

            if (player.playerEquipment.slots[0].amount > 0)
            {
                if (player.playerEquipment.magazineItem.bulletsRemaining != prevMunitionCount)
                {
                    animate = player.playerEquipment.magazineItem.bulletsRemaining == 0;
                    player.playerEquipment.ManageMunition();
                }
                hasWeapon = true;
                if ((prevDurability != player.playerEquipment.slots[0].item.currentDurability))
                {
                    player.playerEquipment.ManageDurability();
                    prevDurability = player.playerEquipment.slots[0].item.currentDurability;
                }
            }
            else
            {
                prevWeaponMunitionCount = 0;
                if (hasWeapon)
                {
                    player.playerEquipment.ManageMunition();
                    hasWeapon = false;
                }
            }
            if(animate)
            {
                if(!scaleAnimation.isAnimating)
                {
                    scaleAnimation.StartAnimation();
                }
            }
        }
    }

    public int TotalMunition(AmmoItem ammoItem)
    {
        int tot = 0;
        if (!player) player = Player.localPlayer;
        if (!player) return 0;
        player.GetComponent<PlayerBelt>().Assign();

        for (int i = 0; i < player.playerBelt.belt.Count; i++)
        {
            if (player.playerBelt.belt[i].amount == 0) continue;
            if (player.playerBelt.belt[i].item.data.name == ammoItem.name)
            {
                tot += player.playerBelt.belt[i].amount;
            }
        }
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            if (player.inventory.slots[i].amount == 0) continue;
            if (player.inventory.slots[i].item.data.name == ammoItem.name)
            {
                tot += player.inventory.slots[i].amount;
            }
        }

        return tot;
    }

    public void Refresh(Player player)
    {
        if (player != null)
        {
            panel.SetActive(true);

            player.playerNavMeshMovement2D.JoystickHandling();
            player.playerNavMeshMovement2D.MoveJoystick();

            // refresh all
            for (int i = 0; i < slots.Length; ++i)
            {
                int index = i;
                ItemSlot itemSlot = player.playerBelt.belt[index];
                UISkillbarSlot slot = slots[index].GetComponent<UISkillbarSlot>();

                if (itemSlot.amount > 0)
                {
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.white;
                    //slot.image.sprite = itemSlot.item.image;
                    slot.image.sprite = itemSlot.item.data.skinImages.Count > 0
                    && itemSlot.item.skin > -1 ? itemSlot.item.data.skinImages[itemSlot.item.skin] : itemSlot.item.data.image;
                    slot.image.preserveAspect = true;
                    slot.cooldownOverlay.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(true);
                    slot.amountText.text = itemSlot.amount.ToString();
                    slot.registerItem.index = index;
                    slot.durabilitySlider.fillAmount = itemSlot.item.data.maxDurability.baseValue > 0 ? ((float)itemSlot.item.currentDurability / (float)itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel)) : 0;
                    slot.unsanitySlider.fillAmount = itemSlot.item.data.maxUnsanity > 0 ? ((float)itemSlot.item.currentUnsanity / (float)itemSlot.item.data.maxUnsanity) : 0;
                    slot.registerItem.skillSlot = true;
                    slot.registerItem.equip = true;
                    slot.registerItem.delete = true;
                    slot.registerItem.use = !(itemSlot.item.data is WaterBottleItem);
                }
                else
                {
                    // refresh empty slot
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownOverlay.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                    slot.registerItem.skillSlot = false;
                    slot.registerItem.index = -1;
                    slot.registerItem.equip = false;
                    slot.registerItem.delete = false;
                    slot.registerItem.use = false;
                    slot.durabilitySlider.fillAmount = 0;
                    slot.unsanitySlider.fillAmount = 0;
                }
            }
        }
    }
}

public partial class PlayerNavMeshMovement2D
{
    public VariableJoystick joystick;
    public VariableJoystick moveJoystick;
    private Vector2 prevJoystick;
    private Vector2 prevMoveJoystick;
    public RaycastHit2D[] hits2D;
    [HideInInspector] public int prevMunitionCountForSound;

    [Client]
    public void MoveJoystick()
    {
        if (player.health.current <= 0) return;
        if (player.playerAdditionalState.additionalState == "SLEEP" && player.playerTired.tired <= 20) return;
        if (player.name != Player.localPlayer.name) return;
        // don't move if currently typing in an input
        // we check this after checking h and v to save computations
        if (!UIUtils.AnyInputActive() && moveJoystick)
        {
            // get horizontal and vertical input
            // 'raw' to start moving immediately. otherwise too much delay.
            // note: no != 0 check because it's 0 when we stop moving rapidly
            float horizontal = moveJoystick.Horizontal;
            float vertical = moveJoystick.Vertical;

            if (horizontal != 0 || vertical != 0)
            {
                // create direction, normalize in case of diagonal movement
                Vector2 direction = new Vector2(horizontal, vertical);
                if (direction.magnitude > 1) direction = direction.normalized;
                if (direction != prevMoveJoystick )
                {
                    player.playerMove.CmdSyncRotation(direction,true);
                    float heading = Mathf.Atan2(direction.x, direction.y);
                    player.playerMove.playerObject.transform.localRotation = Quaternion.Euler(0f, (heading * Mathf.Rad2Deg), 0);
                    if (player.playerMove.states.Contains("AIM") || player.playerMove.states.Contains("SHOOT")) player.playerMove.CmdSetState("", new string[2] { "AIM", "SHOOT" });
                    player.lookDirection = direction;
                    prevMoveJoystick = direction;
                    if (player.skills.currentSkill > -1) player.skills.CmdResetSkill();
                }
                // draw direction for debugging
                Debug.DrawLine(transform.position, transform.position + (Vector3)direction, Color.green, 0, false);

                // clear indicator if there is one, and if it's not on a target
                // (simply looks better)
                if (direction != Vector2.zero)
                    indicator.ClearIfNoParent();

                // cancel path if we are already doing click movement, otherwise
                // we will slide
                agent.ResetMovement();

                // note: SetSpeed() already sets agent.speed to player.speed
                agent.velocity = direction * agent.speed;

                // clear requested skill in any case because if we clicked
                // somewhere else then we don't care about it anymore
                player.useSkillWhenCloser = -1;
            }
        }
    }

    [Client]
    public void RotateByclick()
    {
        if (player.health.current <= 0) return;
        if (player.playerAdditionalState.additionalState == "SLEEP") return;
        if (player.name != Player.localPlayer.name) return;

        // don't move if currently typing in an input
        // we check this after checking h and v to save computations
        if (Input.GetMouseButton(1) && !Utils.IsCursorOverUserInterface())
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0; // In 2D, la posizione z dovrebbe essere zero

            // Ottieni la posizione dell'oggetto
            Vector3 objectPosition = player.transform.GetChild(0).transform.position;

            // Calcola la direzione normalizzata
            Vector2 direction = (mouseWorldPosition - objectPosition); 
            if (direction.magnitude > 1) direction = direction.normalized;
            int temp = player.playerEquipment.magazineItem.bulletsRemaining;//player.playerWeapon.CheckMunitionInMagazine();

            if (temp <= 0)
            {
                if (player.playerEquipment.slots[0].amount > 0)
                {
                    bool itm = player.playerWeapon.CheckMagazine(player.playerEquipment.slots[0].item.data.name);
                    if (itm && prevMunitionCountForSound != temp)
                    {
                        player.playerWeapon.CmdChargeMunition(player.playerEquipment.slots[0].item.name);
                    }
                    else
                    {
                        if (temp == 0 && prevMunitionCountForSound != 0)
                        {
                            if (player.playerEquipment.slots[0].item.data.requiredSkill is MunitionSkill)
                                player.playerSounds.PlaySounds(((MunitionSkill)player.playerEquipment.slots[0].item.data.requiredSkill).projectile.type, "3");
                        }
                    }
                }
            }
            prevMunitionCountForSound = temp;

            if (direction != prevJoystick)
            {
                player.playerMove.CmdSyncRotation(direction, false);
                float heading = Mathf.Atan2(direction.x, direction.y);
                player.playerMove.playerObject.transform.localRotation = Quaternion.Euler(0f, (heading * Mathf.Rad2Deg), 0);
                player.lookDirection = direction;

                prevJoystick = direction;
            }

            if (player.playerMove.canAttack && Player.localPlayer.playerAdditionalState.additionalState == "")
            {
                if (player.playerMove.states.Contains("AIM"))
                {
                    if (player.playerEquipment.slots[0].amount > 0 && ((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo == null)
                    {
                        if (player.playerTired.tired > 0 && player.playerTired.tired <= player.playerTired.tiredLimitForAim && player.mana.current == 0)
                        {
                            player.playerNotification.TargetSpawnNotification("You are too tired to attack!");
                        }
                        else if (player.playerTired.tired > 0 && player.mana.current > 0)
                        {
                            player.playerMove.CmdSetState("SHOOT", new string[1] { "AIM" });
                        }
                    }
                    else
                    {
                        player.playerMove.CmdSetState("SHOOT", new string[1] { "AIM" });
                    }
                }
                if (!player.playerMove.states.Contains("SHOOT"))
                {
                    if (player.playerEquipment.slots[0].amount > 0 && ((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo == null)
                    {
                        if (player.playerTired.tired > 0 && player.playerTired.tired <= player.playerTired.tiredLimitForAim && player.mana.current == 0)
                        {
                            player.playerNotification.TargetSpawnNotification("You are too tired to attack!");
                        }
                        else if (player.playerTired.tired > 0 && player.mana.current > 0)
                        {
                            player.playerMove.CmdSetState("SHOOT", new string[0] { });
                        }
                    }
                    else
                    {
                        player.playerMove.CmdSetState("SHOOT", new string[0] { });
                    }
                }
                if (player.playerEquipment.slots[0].amount > 0 && ((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo == null)
                {
                    if (player.playerTired.tired > 0 && player.mana.current > 0)
                    {
                        ((PlayerSkills)player.skills).TryUse(((PlayerSkills)player.skills).GetSkillIndexByName(player.playerEquipment.slots[0].item.data.requiredSkill.name));
                    }
                }
                else if (player.playerEquipment.slots[0].amount > 0 && ((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo != null)
                {
                    ((PlayerSkills)player.skills).TryUse(((PlayerSkills)player.skills).GetSkillIndexByName(player.playerEquipment.slots[0].item.data.requiredSkill.name));
                }
            }
            else
            {
                if (!player.playerMove.states.Contains("AIM")) player.playerMove.CmdSetState("AIM", new string[1] { "SHOOT" });
            }


            // draw direction for debugging
            //Debug.DrawLine(transform.position, (transform.position + (Vector3)direction * (player.playerEquipment.slots[0].amount > 0 ? player.playerEquipment.slots[0].item.data.attackRange : 0)), Color.green, 0, false);

            // clear indicator if there is one, and if it's not on a target
            // (simply looks better)
            if (direction != Vector2.zero)
                indicator.ClearIfNoParent();

            // cancel path if we are already doing click movement, otherwise
            // we will slide
            agent.ResetMovement();

            // note: SetSpeed() already sets agent.speed to player.speed
            //agent.velocity = direction * agent.speed;

            // clear requested skill in any case because if we clicked
            // somewhere else then we don't care about it anymore
            player.useSkillWhenCloser = -1;
        }
        else
        {
            prevMunitionCountForSound = 1;
            if (player.skills.currentSkill > -1) player.skills.CmdResetSkill();
            if (player.playerMove.states.Contains("SHOOT") || player.playerMove.states.Contains("AIM")) player.playerMove.CmdSetState("", new string[2] { "AIM", "SHOOT" });
        }

    }

    [Client]
    public void JoystickHandling()
    {
        if (player.health.current <= 0) return;
        if (player.playerAdditionalState.additionalState == "SLEEP" ) return;
        if (player.name != Player.localPlayer.name) return;
        if (Input.GetMouseButton(1)) return;

            // don't move if currently typing in an input
            // we check this after checking h and v to save computations
        if (!UIUtils.AnyInputActive() && joystick)
        {
            // get horizontal and vertical input
            // note: no != 0 check because it's 0 when we stop moving rapidly
            float horizontal = joystick.Horizontal;
            float vertical = joystick.Vertical;
            
            if (joystick.input == Vector2.zero)
            {
                prevMunitionCountForSound = 1;
            }           

            if (horizontal != 0 || vertical != 0 && moveJoystick.input != Vector2.zero)
            {
                // create direction, normalize in case of diagonal movement
                Vector2 direction = new Vector2(horizontal, vertical);
                if (direction.magnitude > 1) direction = direction.normalized;
                int temp = player.playerEquipment.magazineItem.bulletsRemaining;//player.playerWeapon.CheckMunitionInMagazine();

                if (temp <= 0 )
                {
                    if (player.playerEquipment.slots[0].amount > 0)
                    {
                        bool itm = player.playerWeapon.CheckMagazine(player.playerEquipment.slots[0].item.data.name);
                        if (itm && prevMunitionCountForSound != temp)
                        {
                            player.playerWeapon.CmdChargeMunition(player.playerEquipment.slots[0].item.name);
                        }
                        else
                        {
                            if (temp == 0 && prevMunitionCountForSound != 0)
                            {
                                if (player.playerEquipment.slots[0].item.data.requiredSkill is MunitionSkill)
                                    player.playerSounds.PlaySounds(((MunitionSkill)player.playerEquipment.slots[0].item.data.requiredSkill).projectile.type, "3");
                            }
                        }
                    }
                }
                prevMunitionCountForSound = temp;

                if (direction != prevJoystick && moveJoystick.input == Vector2.zero)
                {
                    player.playerMove.CmdSyncRotation(direction, false);
                    float heading = Mathf.Atan2(direction.x, direction.y);
                    player.playerMove.playerObject.transform.localRotation = Quaternion.Euler(0f, (heading * Mathf.Rad2Deg), 0);
                    player.lookDirection = direction;

                    prevJoystick = direction;
                }

                if (player.playerMove.canAttack && Player.localPlayer.playerAdditionalState.additionalState == "")
                {
                    if (player.playerMove.states.Contains("AIM"))
                    {
                        if (player.playerEquipment.slots[0].amount > 0 && ((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo == null)
                        {
                            if (player.playerTired.tired > 0 && player.playerTired.tired <= player.playerTired.tiredLimitForAim && player.mana.current == 0)
                            {
                                player.playerNotification.TargetSpawnNotification("You are too tired to attack!");
                            }
                            else if (player.playerTired.tired > 0 && player.mana.current > 0)
                            {
                                player.playerMove.CmdSetState("SHOOT", new string[1] { "AIM" });
                            }
                        }
                        else
                        {
                            player.playerMove.CmdSetState("SHOOT", new string[1] { "AIM" });
                        }
                    }
                    if (!player.playerMove.states.Contains("SHOOT"))
                    {
                        if (player.playerEquipment.slots[0].amount > 0 && ((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo == null)
                        {
                            if (player.playerTired.tired > 0 && player.playerTired.tired <= player.playerTired.tiredLimitForAim && player.mana.current == 0)
                            {
                                player.playerNotification.TargetSpawnNotification("You are too tired to attack!");
                            }
                            else if (player.playerTired.tired > 0 && player.mana.current > 0)
                            {
                                player.playerMove.CmdSetState("SHOOT", new string[0] { });
                            }
                        }
                        else
                        {
                            player.playerMove.CmdSetState("SHOOT", new string[0] { });
                        }
                    }
                    if (player.playerEquipment.slots[0].amount > 0 && ((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo == null)
                    {
                        if (player.playerTired.tired > 0 && player.mana.current > 0) 
                        { 
                            ((PlayerSkills)player.skills).TryUse(((PlayerSkills)player.skills).GetSkillIndexByName(player.playerEquipment.slots[0].item.data.requiredSkill.name));
                        }
                    }
                    else if (player.playerEquipment.slots[0].amount > 0 && ((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo != null)
                    {
                        ((PlayerSkills)player.skills).TryUse(((PlayerSkills)player.skills).GetSkillIndexByName(player.playerEquipment.slots[0].item.data.requiredSkill.name));
                    }
                }
                else
                {
                    if (!player.playerMove.states.Contains("AIM")) player.playerMove.CmdSetState("AIM", new string[1] { "SHOOT" });
                }


                // draw direction for debugging
                //Debug.DrawLine(transform.position, (transform.position + (Vector3)direction * (player.playerEquipment.slots[0].amount > 0 ? player.playerEquipment.slots[0].item.data.attackRange : 0)), Color.green, 0, false);

                // clear indicator if there is one, and if it's not on a target
                // (simply looks better)
                if (direction != Vector2.zero)
                    indicator.ClearIfNoParent();

                // cancel path if we are already doing click movement, otherwise
                // we will slide
                agent.ResetMovement();

                // note: SetSpeed() already sets agent.speed to player.speed
                //agent.velocity = direction * agent.speed;

                // clear requested skill in any case because if we clicked
                // somewhere else then we don't care about it anymore
                player.useSkillWhenCloser = -1;
            }
            else
            {
                if (player.skills.currentSkill > -1) player.skills.CmdResetSkill();
                if (player.playerMove.states.Contains("SHOOT") || player.playerMove.states.Contains("AIM")) player.playerMove.CmdSetState("", new string[2] { "AIM", "SHOOT" });
            }
        }
    }
}
