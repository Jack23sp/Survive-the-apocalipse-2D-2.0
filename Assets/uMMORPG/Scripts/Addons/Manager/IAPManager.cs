using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager singleton;
    IStoreController m_StoreController;
    [SerializeField] ItemMallManager itemMallManager;

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("In-App Purchasing successfully initialized");
        m_StoreController = controller;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        throw new System.NotImplementedException();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        //Retrieve the purchased product
        var product = purchaseEvent.purchasedProduct;

        //Add the purchased product to the players inventory
        //if (product.definition.id == goldProductId)
        //{
        //    AddGold();
        //}
        //else if (product.definition.id == diamondProductId)
        //{
        //    AddDiamond();
        //}

        Debug.Log($"Purchase Complete - Product: {product.definition.id}");

        //We return Complete, informing IAP that the processing on our side is done and the transaction can be closed.
        return PurchaseProcessingResult.Complete;
    }

    void Awake()
    {
        if (!singleton) singleton = this;
        itemMallManager = gameObject.GetComponent<ItemMallManager>();
        InitializePurchasing();
    }

    void InitializePurchasing()
    {

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        for (int i = 0; i < itemMallManager.itemsCode.Count; i++)
        {
            builder.AddProduct(itemMallManager.itemsCode[i], ProductType.Consumable);
        }

        UnityPurchasing.Initialize(this, builder);
    }
}
