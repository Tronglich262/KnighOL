using Assets.HeroEditor.Common.ExampleScripts;
using Ookii.Dialogs;
using System;
using UnityEngine;

public class CanvasShop : MonoBehaviour
{
    public GameObject canvasDaily;
    public GameObject canvasShop;
    public GameObject canvasShopvk;
    public GameObject canvasShopPK;
    public GameObject nv;

    public GameObject panelshoptp;
    public GameObject panelshopvk;
    public GameObject panelshoppk;
    public GameObject diemdanh;
    public static CanvasShop Instante;
    public void Awake()
    {
        Instante = this;
    }
    private void Start()
    {
        if (canvasDaily != null)
        {
            canvasDaily.SetActive(false);
        }
        
        if (canvasShop != null)
        {
            canvasShop.SetActive(false);
        }
        if (nv != null)
        {
            nv.SetActive(false);
        }
        if (canvasShopvk != null)
        {
            canvasShopvk.SetActive(false);
        }
        if (canvasShopPK != null)
        {
            canvasShopPK.SetActive(false);
        }
    }
    public void OnClickDaily()
    {
      canvasDaily.SetActive(!canvasDaily.activeSelf);
        canvasShop.SetActive(false);
        canvasShopvk.SetActive(false);
        nv.SetActive(false);
        canvasShopPK.SetActive(false);
        diemdanh.SetActive(false);
        Tat();
    }
    public void OnClickshop()
    {
      canvasShop.SetActive(!canvasShop.activeSelf);
        canvasDaily.SetActive(false);
        nv.SetActive(false);
        canvasShopvk.SetActive(false);
        canvasShopPK.SetActive(false);
        panelshoptp.SetActive(false);
        Tat();

    }
    public void OnClicknv()
    {
      nv.SetActive(!nv.activeSelf);
        canvasDaily.SetActive(false);
        canvasShop.SetActive(false);
        canvasShopvk.SetActive(false);
        canvasShopPK.SetActive(false);
        diemdanh.SetActive(false);
        Tat();
    }
    public void OnClickshopvk()
    {
      canvasShopvk.SetActive(!canvasShopvk.activeSelf);
        canvasDaily.SetActive(false);
        canvasShop.SetActive(false);
        nv.SetActive(false);
        canvasShopPK.SetActive(false);
        panelshopvk.SetActive(false);
        Tat();
    }
    public void OnClickshoppk()
    {
      canvasShopPK.SetActive(!canvasShopPK.activeSelf);
        canvasDaily.SetActive(false);
        canvasShop.SetActive(false);
        nv.SetActive(false);
        canvasShopvk.SetActive(false);
        panelshoppk.SetActive(false);
        Tat();
    }
    public void ToogleTatTP()
    {
        panelshoptp.SetActive(!panelshoptp.activeSelf);
        Bat();
        bool checktoggle = MovementExample.Instante.checktoggle = false;

    }
    public void ToogleTatpk()
    {
        panelshoppk.SetActive(!panelshoppk.activeSelf);
        Bat();
        bool checktoggle = MovementExample.Instante.checktoggle =false;
    }
    public void ToogleTatvk()
    {
        panelshopvk.SetActive(!panelshopvk.activeSelf);
        Bat();
        bool checktoggle = MovementExample.Instante.checktoggle = false;

    }
    public void ToogleTatdiemdanh()
    {
        diemdanh.SetActive(!diemdanh.activeSelf);
        Bat();
        bool checktoggle = MovementExample.Instante.checktoggle = false;

    }
    public void ToogleTatTPshop()
    {
        canvasShop.SetActive(false);
        panelshoptp.SetActive(true);
        Bat();

    }
    public void ToogleTatpkshop()
    {
        canvasShopPK.SetActive(false);
        panelshoppk.SetActive(true);
        Bat();

    }
    public void ToogleTatvkshop()
    {
        canvasShopvk.SetActive(false);
        panelshopvk.SetActive(true);
        Bat();

    }
    public void ToogleTatdiemdanhshop()
    {
        nv.SetActive(false);
        diemdanh.SetActive(true);
        Bat();

    }
    public void ToogleTatdiemdanhshop1()
    {
        canvasDaily.SetActive(false);
        diemdanh.SetActive(true);
        Bat();

    }
    //( cấm động )
    public void Tat()
    {
        CharacterUIManager.Instance.ToggleTatCharacterButton();
        WorldChatUIManager.Instance.ToggleTatCharbarAndChatPrivateList();
    }
    public void Bat()
    {
        CharacterUIManager.Instance.ToggleBatCharacterButton();
        WorldChatUIManager.Instance.ToggleBatCharbarAndChatPrivateList();
    }
}