using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class IOSBuildConfigurator
{
    private const string BundleIdentifier =
        "com.fantasystudio.vietnamesebird";

    private const string ProductName =
        "Vietnamese Birds";

    private const string CompanyName =
        "Fantasy Studio";

    /// <summary>
    /// Được GitHub Actions gọi trước khi xuất Xcode project.
    /// </summary>
    public static void Configure()
    {
        Debug.Log(
            "IOS CONFIG: Bắt đầu cấu hình Player Settings cho iOS."
        );

        // -----------------------------------------------------
        // THÔNG TIN ỨNG DỤNG
        // -----------------------------------------------------

        PlayerSettings.companyName = CompanyName;
        PlayerSettings.productName = ProductName;

        PlayerSettings.SetApplicationIdentifier(
            BuildTargetGroup.iOS,
            BundleIdentifier
        );

        PlayerSettings.bundleVersion = "1.0.0";

        // Build number sẽ được workflow ghi đè
        // bằng github.run_number.
        PlayerSettings.iOS.buildNumber = "1";

        // -----------------------------------------------------
        // CẤU HÌNH IOS
        // -----------------------------------------------------

        PlayerSettings.iOS.sdkVersion =
            iOSSdkVersion.DeviceSDK;

        PlayerSettings.iOS.targetDevice =
            iOSTargetDevice.iPhoneOnly;

        PlayerSettings.iOS.targetOSVersionString =
            "13.0";

        PlayerSettings.SetScriptingBackend(
            NamedBuildTarget.iOS,
            ScriptingImplementation.IL2CPP
        );

        // -----------------------------------------------------
        // KHÓA MÀN HÌNH DỌC
        // -----------------------------------------------------

        PlayerSettings.defaultInterfaceOrientation =
            UIOrientation.Portrait;

        PlayerSettings.allowedAutorotateToPortrait =
            true;

        PlayerSettings.allowedAutorotateToPortraitUpsideDown =
            false;

        PlayerSettings.allowedAutorotateToLandscapeLeft =
            false;

        PlayerSettings.allowedAutorotateToLandscapeRight =
            false;

        // -----------------------------------------------------
        // HIỂN THỊ TOÀN MÀN HÌNH
        // -----------------------------------------------------

        PlayerSettings.statusBarHidden = true;

        // -----------------------------------------------------
        // LƯU THAY ĐỔI
        // -----------------------------------------------------

        AssetDatabase.SaveAssets();

        Debug.Log(
            "IOS CONFIG: Cấu hình hoàn tất." +
            "\nBundle ID: " + BundleIdentifier +
            "\nProduct Name: " + ProductName +
            "\nMinimum iOS: 13.0" +
            "\nTarget Device: iPhone Only" +
            "\nOrientation: Portrait" +
            "\nScripting Backend: IL2CPP"
        );
    }
}