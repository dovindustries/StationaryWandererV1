using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Oculus.Installer.Editor.Utils;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Oculus.Installer.Editor
{
    public class OculusIntegrationInstallerWindow : EditorWindow
    {
        private const string LicenseURL = "https://developer.oculus.com/licenses/oculussdk";

        private const string FeedbackURL =
                "https://forums.oculusvr.com/t5/SDK-Feedback/bd-p/dev-sdk-tool-feedback";

        private static readonly Vector2 WindowSize = new Vector2(450, 148);

        private static readonly StyleColor LinkColor =
                new StyleColor(new Color32(29, 101, 193, 255));

        private static readonly StyleColor LinkHoverColor =
                new StyleColor(new Color32(44, 132, 193, 255));

        private Button _installButton;
        private Toggle _licenseCheckbox;
        private OculusIntegrationRegistryInstaller.StatusCode _backingStatus;
        private Button _licenseLink;

        private float _reloadProgress = -1f;

        // Add a toolbar menu item
        [MenuItem("Oculus/Install Oculus Integration")]
        public static void ShowWindow()
        {
            var window =
                    GetWindow<OculusIntegrationInstallerWindow>(
                            title: "Oculus Integration",
                            focus: true);
            window.minSize = WindowSize;
            window.maxSize = WindowSize;
            window.Show();
        }

        private void CreateGUI()
        {
            var container = new VisualElement
            {
                    style =
                    {
                            paddingLeft = 8,
                            paddingTop = 8,
                            paddingRight = 8,
                            paddingBottom = 8,
                            flexDirection = FlexDirection.Column,
                            justifyContent = Justify.FlexStart,
                            alignItems = Align.Stretch,
                            flexGrow = 1f,
                    }
            };

            var logoImage = new Image
            {
                    image = Resources.Load<Texture>("OculusLogo"),
                    style =
                    {
                            width = 128,
                            height = 28,
                            marginBottom = 4,
                            alignSelf = Align.FlexStart
                    },
            };
            container.Add(logoImage);

            // License agreement checkbox
            var licenseContainer = new VisualElement
            {
                    style =
                    {
                            marginTop = 4,
                            marginBottom = 4,
                            flexDirection = FlexDirection.Row,
                            alignItems = Align.Center,
                            justifyContent = Justify.FlexStart
                    }
            };
            _licenseCheckbox = new Toggle
            {
                    style =
                    {
                            width = 24
                    },
                    text = null,
            };
            licenseContainer.Add(_licenseCheckbox);
            var textElem = new TextElement
            {
                    text = "I have read and agree to the terms of the"
            };
            licenseContainer.Add(textElem);
            _licenseLink = CreateHyperlinkButton("Oculus SDK license", LicenseURL);
            licenseContainer.Add(_licenseLink);
            container.Add(licenseContainer);

            _installButton = new Button(OnInstallButtonClick)
            {
                    text = "Add Oculus Integration to Package Manager",
                    style =
                    {
                            paddingLeft = 8,
                            paddingTop = 8,
                            paddingRight = 8,
                            paddingBottom = 8,
                            marginTop = 4,
                            fontSize = 14
                    }
            };
            container.Add(_installButton);

            var footer = new VisualElement
            {
                    style =
                    {
                            height = 20,
                            marginTop = 4,
                            flexDirection = FlexDirection.Row,
                            alignContent = Align.Stretch,
                    }
            };
            var versionText = new TextElement
            {
                    text = "Experimental",
                    style =
                    {
                            color = new Color(0.5f, 0.5f, 0.5f),
                            width = 120,
                            height = 20,
                            unityTextAlign = TextAnchor.MiddleLeft,
                    }
            };
            footer.Add(versionText);

            var feedbackLink = CreateHyperlinkButton("Feedback", FeedbackURL);
            feedbackLink.style.width = 64;
            feedbackLink.style.height = 20;
            feedbackLink.style.unityTextAlign = TextAnchor.MiddleRight;
            feedbackLink.style.flexGrow = 1;
            footer.Add(feedbackLink);
            container.Add(footer);

            rootVisualElement.Add(container);
        }

        private void Update()
        {
            _licenseCheckbox.SetEnabled(
                    !OculusIntegrationRegistryInstaller.IsRunning &&
                    _reloadProgress < 0);
            _installButton.SetEnabled(
                    _licenseCheckbox.value &&
                    !OculusIntegrationRegistryInstaller.IsRunning &&
                    _reloadProgress < 0);

            if (_reloadProgress >= 0)
            {
                EditorUtility.DisplayProgressBar(
                        "Loading",
                        "Loading packages...",
                        _reloadProgress);
            }

            if (_backingStatus == OculusIntegrationRegistryInstaller.Status)
            {
                return;
            }

            _backingStatus = OculusIntegrationRegistryInstaller.Status;

            if (OculusIntegrationRegistryInstaller.IsRunning)
            {
                EditorUtility.DisplayProgressBar(
                        "Importing Oculus Integration",
                        "Importing Oculus Integration package registries...",
                        0);
            } else if (OculusIntegrationRegistryInstaller.IsError)
            {
                PromptError(OculusIntegrationRegistryInstaller.StatusMessage);
            }

            if (OculusIntegrationRegistryInstaller.IsSuccess)
            {
                OnInstallSuccess();
            }
        }

        private async void OnInstallSuccess()
        {
            _reloadProgress = 0f;

            // Force a resolve of Package Manager. If the registries were successfully installed, Unity will show a
            // corresponding prompt and bring up package registry settings.
            Client.Resolve();

            // Load all packages and find ones belonging to the imported registry
            _reloadProgress = 0.1f;
            var searchRequest = Client.SearchAll();
            _reloadProgress = 0.5f;
            await AsyncUtils.WaitUntilAsync(
                    CancellationToken.None,
                    () => searchRequest.IsCompleted);
            _reloadProgress = 0.95f;
            await Task.Delay(1000);
            _reloadProgress = -1f;

            if (searchRequest.Status == StatusCode.Failure)
            {
                PromptError(
                        "Package manager failed to search for packages. Please check your internet connection.");
                return;
            }

            if (searchRequest.Result == null || searchRequest.Result.Length == 0)
            {
                PromptError("No package found in package manager. Please check your Unity installation.");
                return;
            }

            var installerRegistryUrl = new Uri(OculusIntegrationRegistryInstaller.ServerUrl);
            var foundPackages =
                    (from p in searchRequest.Result
                            let packageRegistryUrl = new Uri(p.registry.url)
                            where packageRegistryUrl.Equals(
                                    installerRegistryUrl)
                            select p)
                    .ToList();

            if (foundPackages.Count == 0)
            {
                PromptError($"No package found in registry. Please verify that the following is reachable:\n\t{OculusIntegrationRegistryInstaller.ServerUrl}");
                return;
            }

            string foundPackagesStr = foundPackages.Aggregate(
                    "",
                    (current, p) => current + "\t- " + p.name + "\n");

            // If package scope found: prompt to open the Package Manager window
            bool openPackageManager = EditorUtility.DisplayDialog(
                    "Success",
                    "Successfully imported Oculus Integration Package Registry. Please install needed packages in Package Manager.\nPackages available: \n\n" +
                    foundPackagesStr,
                    "Open Package Manager",
                    "Dismiss");
            if (openPackageManager)
            {
                Window.Open(foundPackages[0].name);
            }
        }

        private static void OnInstallButtonClick()
        {
            OculusIntegrationRegistryInstaller.StartInstallation();
        }

        private static void PromptError(string message)
        {
            EditorUtility.DisplayDialog(
                    "Failed to Import Oculus Integration SDKs",
                    message,
                    "Dismiss");
            Debug.LogError(
                    "Failed to import Oculus Integration SDKs: " + message);
        }

        private static Button CreateHyperlinkButton(string label, string url)
        {
            var button = new Button(() => Application.OpenURL(url))
            {
                    text = label,
                    style =
                    {
                            color = LinkColor,
                            backgroundImage = null,
                            backgroundColor = Color.clear,
                            borderTopColor = Color.clear,
                            borderRightColor = Color.clear,
                            borderBottomColor = Color.clear,
                            borderLeftColor = Color.clear,
                            paddingLeft = 0,
                            paddingRight = 0
                    },
                    tooltip = url
            };
            button.RegisterCallback((MouseOverEvent _) =>
            {
                button.style.color = LinkHoverColor;
            });
            button.RegisterCallback((MouseOutEvent _) =>
            {
                button.style.color = LinkColor;
            });
            return button;
        }
    }
}
