using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using PimDeWitte.UnityMainThreadDispatcher;
using PortalToUnity;
using TMPro;
using UnityEngine;

public class RichesManager : MonoBehaviour
{
    public PortalOfPower Portal;
    [HideInInspector] public RichesManager Instance;

    [SerializeField] private float speed;
    [SerializeField] private float mult;
    [SerializeField] private Transform placeFigureText;
    [SerializeField] private GameObject alert;
    [SerializeField] private TextMeshProUGUI alertText;
    [SerializeField] private TextMeshProUGUI alertDescription;
    private PortalFigure alertedFigure;
    private CancellationTokenSource cts = new CancellationTokenSource();
    
    private List<PortalFigure> finishedFigures = new List<PortalFigure>();

    private void OnEnable()
    {
        PortalOfPower.OnAdded += PortalAdded;
        PortalOfPower.OnRemoved += PortalRemoved;
    }

    private void OnDisable()
    {
        PortalOfPower.OnAdded -= PortalAdded;
        PortalOfPower.OnRemoved -= PortalRemoved;
    }

    private void Awake()
    {
        Instance = this;
        alertText.text = "No Portal Found";
        alertDescription.text = "Please plug in a Portal of Power with the libusbK driver";
        alert.SetActive(true);
    }

    private void Update()
    {
        float sinValue = Mathf.Sin(Time.time * speed);
        placeFigureText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, sinValue * mult);
        Debug.Log(alertedFigure != null);
        Debug.Log(finishedFigures.Count);
    }

    public bool ShowAlert(string title, bool force = true)
    {
        bool result = true;
        if (cts.IsCancellationRequested)
        {
            if (!force)
                return false;
            else
                result = false;
        }

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            alertText.text = title;
            alert.SetActive(true);
        });
        return result;
    }

    public bool ShowAlert(string title, string message, bool force = true)
    {
        bool result = true;
        if (cts.IsCancellationRequested)
        {
            if (!force)
                return false;
            else
                result = false;
        }

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            alertText.text = title;
            alertDescription.text = message;
            alert.SetActive(true);
        });
        return result;
    }

    public void HideAlert()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (PortalOfPower.Instances.Count == 0)
            {
                ShowAlert("No Portal Found", "Please plug in a Portal of Power with the libusbK driver");
                return;
            }
            if (finishedFigures.Count > 0)
            {
                (ToyCode characterID, VariantID variantID) = GetCharacterAndVariantIDs(finishedFigures[0]);
                Skylander skylander = SkylanderDatabase.GetSkylander(characterID);
                
                alertedFigure = finishedFigures[0];

                switch (finishedFigures[0].recogniseState)
                {
                    case RichesRecogniseState.Normal:
                        ShowAlert("Finished Writing", $"You can now remove {skylander.Name} from the Portal of Power");
                        Portal?.COMMAND_SetLEDColor(0xFF, 0xFF, 0xFF);
                        break;
                    
                    case RichesRecogniseState.Unsupported:
                        ShowAlert("Unsupported Skylander", $"Please remove {skylander.Name} from the Portal of Power");
                        Portal?.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                        break;
                    
                    case RichesRecogniseState.Unknown:
                        ShowAlert("Unknown Skylander", "Please remove this Skylander from the Portal of Power");
                        Portal?.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                        break;
                    
                    case RichesRecogniseState.NotASkylander:
                        ShowAlert("Unknown Toy", "Please remove this toy from the Portal of Power");
                        Portal?.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                        break;
                }
                return;
            }
            alertedFigure = null;
            alert.SetActive(false);
            Portal?.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
        });
    }

    private unsafe (ToyCode, VariantID) GetCharacterAndVariantIDs(PortalFigure figure)
    {
        ToyCode characterID = (ToyCode)figure.TagHeader->toyType;
        VariantID variantID = new VariantID(figure.TagHeader->subType);
        return (characterID, variantID);
    }

    private async Task DoFigure(PortalFigure figure)
    {
        figure.Parent.currentlyQueryingFigure = figure;
        cts?.Cancel();
        cts = new CancellationTokenSource();

        try
        {
            await figure.FetchTagHeader();
            (ToyCode characterID, VariantID variantID) = GetCharacterAndVariantIDs(figure);
            alertedFigure = figure;

            if (!Enum.IsDefined(typeof(ToyCode), characterID))
            {
                if (figure.Parent.FiguresInQueue.Count == 0)
                {
                    figure.recogniseState = RichesRecogniseState.Unknown;
                    if (!ShowAlert("Unknown Skylander", "Please remove this Skylander from the Portal of Power")) throw new Exception();
                    Portal?.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                }
                finishedFigures.Insert(0, figure);
                return;
            }

            Skylander skylander = SkylanderDatabase.GetSkylander(characterID);

            switch (skylander.Type)
            {
                case SkyType.Skylander:
                case SkyType.Giant:
                case SkyType.TrapMaster:
                case SkyType.SuperCharger:
                case SkyType.Sensei:
                case SkyType.Mini:
                case SkyType.SWAPForceTop:
                    if (!ShowAlert("Reading", $"Giving {skylander.Name} max money. Please wait", false)) throw new Exception();
                    Portal?.COMMAND_SetLEDColor(PortalElements.colors[skylander.Element].Color);

                    figure.TagBuffer = new FigType_Skylander(figure);
                    FigType_Skylander sky = (FigType_Skylander)figure.TagBuffer;

                    await sky.FetchMagicMoment0();
                    await sky.FetchRemainingData0();

                    unsafe { sky.SpyroTag->magicMomentRegion0.money = ushort.MaxValue; }

                    if (!ShowAlert("Writing", false)) throw new Exception();

                    await sky.SetMagicMoment0();
                    await sky.SetRemainingData0();

                    finishedFigures.Insert(0, figure);

                    if (figure.Parent.FiguresInQueue.Count == 0)
                    {
                        if (!ShowAlert("Finished Writing", $"You can now remove {skylander.Name} from the Portal of Power", false)) throw new Exception();
                        Portal?.COMMAND_SetLEDColor(0xFF, 0xFF, 0xFF);
                    }
                    break;
                
                case SkyType.CreationCrystal:
                case SkyType.Imaginator:
                    if (!ShowAlert("Reading", $"Giving {skylander.Name} max money. Please wait", false)) throw new Exception();
                    Portal?.COMMAND_SetLEDColor(PortalElements.colors[skylander.Element].Color);

                    figure.TagBuffer = new FigType_CreationCrystal(figure);
                    FigType_CreationCrystal cc = (FigType_CreationCrystal)figure.TagBuffer;

                    await cc.FetchMagicMoment();
                    await cc.FetchRemainingData();

                    unsafe { cc.SpyroTag->magicMoment.money = ushort.MaxValue; }

                    if (!ShowAlert("Writing", false)) throw new Exception();

                    await cc.SetMagicMoment();
                    await cc.SetRemainingData();

                    finishedFigures.Insert(0, figure);

                    if (figure.Parent.FiguresInQueue.Count == 0)
                    {
                        if (!ShowAlert("Finished Writing", $"You can now remove {skylander.Name} from the Portal of Power", false)) throw new Exception();
                        Portal?.COMMAND_SetLEDColor(0xFF, 0xFF, 0xFF);
                    }
                    break;
                
                case SkyType.SWAPForceBottom:
                    break;
                
                default:
                    figure.recogniseState = RichesRecogniseState.Unsupported;
                    finishedFigures.Insert(0, figure);

                    if (figure.Parent.FiguresInQueue.Count == 0)
                    {
                        if (!ShowAlert("Unsupported Skylander", $"Please remove {skylander.Name} from the Portal of Power", false)) throw new Exception();
                        Portal?.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                    }
                    break;
            }
        }
        catch (FigureRemovedException ex)
        {
            Debug.LogException(ex);
            HideAlert();
        }
        catch (FigureErrorException ex)
        {
            Debug.LogException(ex);
            figure.recogniseState = RichesRecogniseState.NotASkylander;

            if (figure.Parent != null)
                finishedFigures.Insert(0, figure);

            if (figure.Parent.FiguresInQueue.Count == 0)
            {
                alertedFigure = figure;
                if (!ShowAlert("Unknown Toy", "Please remove this toy from the Portal of Power", false)) return;
                Portal?.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
            }
        }
        catch (CryptographicException ex)
        {
            Debug.LogException(ex);
            ShowAlert("No Salt Provided", $"No correct salt.txt could be found at \"Assets/StreamingAssets/\". Please add the file and close the program");
        }
        catch (Exception)
        {
            alertedFigure = null;
            if (finishedFigures.Contains(figure))
                finishedFigures.Remove(figure);
        }
        finally
        {
            figure.Parent.currentlyQueryingFigure = null;
            await FigureNoLongerBeingQueried(figure);
            cts = new CancellationTokenSource();
        }
    }

    private async Task FigureNoLongerBeingQueried(PortalFigure figure)
    {
        if (figure.Parent.FiguresInQueue.Count > 0 && PortalOfPower.Instances.Count == 1)
        {
            PortalFigure queuedFigure = figure.Parent.FiguresInQueue[0];
            figure.Parent.FiguresInQueue.Remove(figure.Parent.FiguresInQueue[0]);
            await DoFigure(queuedFigure);
        }
    }

    private async void PortalAdded(PortalOfPower portal)
    {
        if (PortalOfPower.Instances.Count == 1)
        {
            HideAlert();
            Portal = portal;
            Portal.OnFigureAdded += PortalFigureAdded;
            Portal.OnFigureRemoved += PortalFigureRemoved;
            Portal.OnInputReport += PortalInputReport;
            await SetUpPortal(portal);

            if (portal?.FiguresInQueue.Count > 0 && Portal == portal)
            {
                PortalFigure queuedFigure = Portal?.FiguresInQueue[0];
                Portal?.FiguresInQueue.Remove(Portal?.FiguresInQueue[0]);
                await DoFigure(queuedFigure);
            }
        }
        else
        {
            ShowAlert("Too Many Portals", "Please ensure only one Portal of Power is plugged in at a time");
            await SetUpPortal(portal);
        }
    }

    private async void PortalRemoved(PortalOfPower portal)
    {
        portal?.bootCTS?.Cancel();

        if (alertedFigure?.Parent == portal)
            alertedFigure = null;
        
        for (int i = 0; i < finishedFigures.Count; i++)
        {
            if (finishedFigures[i].Parent == portal)
                finishedFigures.Remove(finishedFigures[i]);
        }

        if (portal == Portal)
        {
            cts?.Cancel();
            portal.OnFigureAdded -= PortalFigureAdded;
            portal.OnFigureRemoved -= PortalFigureRemoved;
            portal.OnInputReport -= PortalInputReport;

            if (PortalOfPower.Instances.Count > 1 && PortalOfPower.Instances[1] != null)
            {
                PortalOfPower _portal = PortalOfPower.Instances[1];
                Portal = _portal;
                HideAlert();
                
                _portal.OnFigureAdded += PortalFigureAdded;
                _portal.OnFigureRemoved += PortalFigureRemoved;
                _portal.OnInputReport += PortalInputReport;
                
                await SetUpPortal(_portal);
                
                if (Portal?.FiguresInQueue.Count > 0 && Portal == _portal)
                {
                    PortalFigure queuedFigure = Portal?.FiguresInQueue[0];
                    Portal?.FiguresInQueue.Remove(Portal?.FiguresInQueue[0]);
                    await DoFigure(queuedFigure);
                }
                return;
            }
            ShowAlert("No Portal Found", "Please plug in a Portal of Power with the libusbK driver");
            Portal = null;
            return;
        }
        if (PortalOfPower.Instances.Count == 2)
            HideAlert();
    }

    private async Task SetUpPortal(PortalOfPower portal)
    {
        portal.State = PortalState.SetUpForInterface;
        int timeout = 0;
        portal.bootCTS = new CancellationTokenSource();

        while (true)
        {
            if (timeout == 10)
            {
                Debug.LogError("Portal failing to return info! Please unplug the Portal of Power");
                return;
            }

            portal?.COMMAND_ResetPortal();

            try
            {
                await Task.Delay(50 + (timeout * 100), portal.bootCTS.Token);
            }
            catch (OperationCanceledException) { break; }

            if (portal == null || !PortalOfPower.Instances.Contains(portal)) 

            if (portal == null || !PortalOfPower.Instances.Contains(portal)) return;

            if (portal?.State > PortalState.SetUpForInterface)
                break;
            
            timeout++;
        }

        timeout = 0;
        portal?.COMMAND_ResetPortal();
        portal.bootCTS = new CancellationTokenSource();
        
        while (true)
        {
            if (timeout == 5)
            {
                Debug.LogError("Portal failing to return info! Please unplug the Portal of Power");
                return;
            }

            portal?.COMMAND_SetAntenna(true);

            try
            {
                await Task.Delay(50 + (timeout * 100), portal.bootCTS.Token);
            }
            catch (OperationCanceledException) { break; }

            if (portal == null || !PortalOfPower.Instances.Contains(portal)) return;

            if (portal?.State == PortalState.Standby) break;

            timeout++;
        }

        if (portal == null || !PortalOfPower.Instances.Contains(portal)) return;

        portal.readFigures = true;
        portal.State = PortalState.Ready;
    }

    private async void PortalFigureAdded(PortalFigure figure)
    {
        if (figure.Parent == Portal && PortalOfPower.Instances.Count == 1)
        {
            if (figure.Parent.currentlyQueryingFigure != null || !figure.Parent.readFigures)
            {
                if (figure.Parent.State < PortalState.Standby) return;
                figure.Parent.FiguresInQueue.Add(figure);
                return;
            }
            await DoFigure(figure);
        }
    }

    private async void PortalFigureRemoved(PortalFigure figure)
    {
        if (figure.Parent.FiguresInQueue.Contains(figure))
            figure.Parent.FiguresInQueue.Remove(figure);

        if (figure.Parent.currentlyQueryingFigure == figure)
        {
            figure.Parent.currentlyQueryingFigure = null;
            await FigureNoLongerBeingQueried(figure);
        }

        if (finishedFigures.Contains(figure))
        {
            finishedFigures.Remove(figure);
            if (alertedFigure == figure)
            {
                alertedFigure = null;
                HideAlert();
            }
        }
    }

    private void PortalInputReport(PortalOfPower portal, byte[] data)
    {
        if ((char)data[0] == 'A')
        {
            if (portal?.State == PortalState.CommunicatingWithAntenna)
            {
                portal?.bootCTS.Cancel();
                portal.State = PortalState.Standby;
            }
        }
        else if ((char)data[0] == 'R')
        {
            if (portal?.State == PortalState.SetUpForInterface)
            {
                portal?.bootCTS.Cancel();
                portal.State = PortalState.CommunicatingWithAntenna;
            }
            
            foreach (PortalFigure fig in portal?.Figures)
                fig.Presence = FigurePresence.NotPresent;
        }
    }
}
