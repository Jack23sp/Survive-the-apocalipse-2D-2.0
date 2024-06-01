using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPetStatusCustom : MonoBehaviour
{
    public static UIPetStatusCustom singleton;

    public GameObject panel;
    public Image image;
    public Image healthSlider;
    public Image experienceSlider;
    public Button manageButton;
    private SpriteRenderer spriteRenderer;
    public UIPetStatusManagement petManagement;


    public void Start()
    {
        if (!singleton) singleton = this;

        manageButton.onClick.RemoveAllListeners();
        manageButton.onClick.AddListener(() =>
        {
            petManagement.Open();
        });
    }

    void Update()
    {
        Player player = Player.localPlayer;

        if (player != null &&
            player.petControl.activePet != null)
        {
            Pet pet = player.petControl.activePet;
            panel.SetActive(true);

            if (!spriteRenderer) spriteRenderer = pet.GetComponent<SpriteRenderer>();

            image.sprite = spriteRenderer.sprite;
            image.preserveAspect = true;

            healthSlider.fillAmount = pet.health.Percent();
            experienceSlider.fillAmount = pet.experience.Percent();
        }
        else
        {
            spriteRenderer = null;
            panel.SetActive(false);
        }
    }
}
