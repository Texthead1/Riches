using System;
using System.Collections.Generic;
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
    }

    public void ShowAlert(string title, string message)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            alertText.text = title;
            alertDescription.text = message;
            alert.SetActive(true);
        });
    }

    public void HideAlert()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (finishedFigures.Count > 0)
            {
                (ToyCode characterID, VariantID variantID) = GetCharacterAndVariantIDs(finishedFigures[0]);
                Skylander skylander = SkylanderDatabase.GetSkylander(characterID);
                
                alertedFigure = finishedFigures[0];

                switch (finishedFigures[0].recogniseState)
                {
                    case RichesRecogniseState.Normal:
                        ShowAlert("Finished Writing", $"You can now remove {skylander.Name} from the Portal of Power");
                        Portal.COMMAND_SetLEDColor(0xFF, 0xFF, 0xFF);
                        break;
                    
                    case RichesRecogniseState.Unsupported:
                        ShowAlert("Unsupported Skylander", $"Please remove {skylander.Name} from the Portal of Power");
                        Portal.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                        break;
                    
                    case RichesRecogniseState.Unknown:
                        ShowAlert("Unknown Skylander", "Please remove this Skylander from the Portal of Power");
                        Portal.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                        break;
                    
                    case RichesRecogniseState.NotASkylander:
                        ShowAlert("Unknown Toy", "Please remove this toy from the Portal of Power");
                        Portal.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                        break;
                }
                return;
            }
            alertedFigure = null;
            alert.SetActive(false);
            Portal.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
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
                    Portal.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                    ShowAlert("Unknown Skylander", "Please remove this Skylander from the Portal of Power");
                }
                finishedFigures.Add(figure);
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
                    Portal.COMMAND_SetLEDColor(PortalElements.colors[skylander.Element].Color);
                    ShowAlert("Reading", $"Giving {skylander.Name} max money. Please wait");

                    figure.TagBuffer = new FigType_Skylander(figure);
                    FigType_Skylander sky = (FigType_Skylander)figure.TagBuffer;

                    await sky.FetchMagicMoment0();
                    await sky.FetchRemainingData0();

                    unsafe { sky.SpyroTag->magicMomentRegion0.money = ushort.MaxValue; }

                    ShowAlert("Writing", $"Giving {skylander.Name} max money. Please wait");

                    await sky.SetMagicMoment0();
                    await sky.SetRemainingData0();

                    finishedFigures.Add(figure);

                    if (figure.Parent.FiguresInQueue.Count == 0)
                    {
                        Portal.COMMAND_SetLEDColor(0xFF, 0xFF, 0xFF);
                        ShowAlert("Finished Writing", $"You can now remove {skylander.Name} from the Portal of Power");
                    }
                    break;
                
                case SkyType.CreationCrystal:
                case SkyType.Imaginator:
                    Portal.COMMAND_SetLEDColor(PortalElements.colors[skylander.Element].Color);
                    ShowAlert("Reading", skylander.Name);

                    figure.TagBuffer = new FigType_CreationCrystal(figure);
                    FigType_CreationCrystal cc = (FigType_CreationCrystal)figure.TagBuffer;

                    await cc.FetchMagicMoment();
                    await cc.FetchRemainingData();

                    unsafe { cc.SpyroTag->magicMoment.money = ushort.MaxValue; }

                    ShowAlert("Writing", $"Giving {skylander.Name} max money. Please wait");

                    await cc.SetMagicMoment();
                    await cc.SetRemainingData();

                    finishedFigures.Add(figure);

                    if (figure.Parent.FiguresInQueue.Count == 0)
                    {
                        Portal.COMMAND_SetLEDColor(0xFF, 0xFF, 0xFF);
                        ShowAlert("Finished Writing", $"You can now remove {skylander.Name} from the Portal of Power");
                    }
                    
                    break;
                
                case SkyType.SWAPForceBottom:
                    break;
                
                default:
                    figure.recogniseState = RichesRecogniseState.Unsupported;
                    finishedFigures.Add(figure);

                    if (figure.Parent.FiguresInQueue.Count == 0)
                    {
                        Portal.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                        ShowAlert("Unsupported Skylander", $"Please remove {skylander.Name} from the Portal of Power");
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
            finishedFigures.Add(figure);

            if (figure.Parent.FiguresInQueue.Count == 0)
            {
                alertedFigure = figure;
                Portal.COMMAND_SetLEDColor(0x00, 0x00, 0x00);
                ShowAlert("Unknown Toy", "Please remove this toy from the Portal of Power");
            }
        }
        finally
        {
            figure.Parent.currentlyQueryingFigure = null;
            await FigureNoLongerBeingQueried(figure);
        }
    }

    private async Task FigureNoLongerBeingQueried(PortalFigure figure)
    {
        if (figure.Parent.FiguresInQueue.Count > 0)
        {
            PortalFigure queuedFigure = figure.Parent.FiguresInQueue[0];
            figure.Parent.FiguresInQueue.Remove(figure.Parent.FiguresInQueue[0]);
            await DoFigure(queuedFigure);
        }
    }

    private async void PortalAdded(PortalOfPower portal)
    {
        Debug.Log("portal added");
        if (PortalOfPower.Instances.Count == 1)
        {
            HideAlert();
            Portal = portal;
            portal.OnFigureAdded += PortalFigureAdded;
            portal.OnFigureRemoved += PortalFigureRemoved;
            portal.OnInputReport += PortalInputReport;
            await SetUpPortal(portal);

            if (portal.FiguresInQueue.Count > 0 && Portal == portal)
            {
                PortalFigure queuedFigure = portal.FiguresInQueue[0];
                portal.FiguresInQueue.Remove(portal.FiguresInQueue[0]);
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
        Debug.Log("portal removed");
        portal.bootCTS.Cancel();

        if (portal == Portal)
        {
            Portal.OnFigureAdded -= PortalFigureAdded;
            Portal.OnFigureRemoved -= PortalFigureRemoved;
            portal.OnInputReport -= PortalInputReport;

            if (PortalOfPower.Instances.Count > 1 && PortalOfPower.Instances[1] != null)
            {
                Portal = PortalOfPower.Instances[1];
                HideAlert();
                
                portal.OnFigureAdded += PortalFigureAdded;
                portal.OnFigureRemoved += PortalFigureRemoved;
                portal.OnInputReport += PortalInputReport;
                
                await SetUpPortal(Portal);
                
                if (portal.FiguresInQueue.Count > 0 && Portal == portal)
                {
                    PortalFigure queuedFigure = portal.FiguresInQueue[0];
                    portal.FiguresInQueue.Remove(portal.FiguresInQueue[0]);
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

            portal.COMMAND_ResetPortal();

            try
            {
                await Task.Delay(50 + (timeout * 100), portal.bootCTS.Token);
            }
            catch (OperationCanceledException) { break; }

            if (portal == null || !PortalOfPower.Instances.Contains(portal)) 

            if (portal == null || !PortalOfPower.Instances.Contains(portal)) return;

            if (portal.State > PortalState.SetUpForInterface)
                break;
            
            timeout++;
        }

        timeout = 0;
        portal.COMMAND_ResetPortal();
        portal.bootCTS = new CancellationTokenSource();
        
        while (true)
        {
            if (timeout == 5)
            {
                Debug.LogError("Portal failing to return info! Please unplug the Portal of Power");
                return;
            }

            portal.COMMAND_SetAntenna(true);

            try
            {
                await Task.Delay(50 + (timeout * 100), portal.bootCTS.Token);
            }
            catch (OperationCanceledException) { break; }

            if (portal == null || !PortalOfPower.Instances.Contains(portal)) return;

            if (portal.State == PortalState.Standby) break;

            timeout++;
        }

        if (portal == null || !PortalOfPower.Instances.Contains(portal)) return;

        portal.readFigures = true;
        portal.State = PortalState.Ready;
    }

    private void PortalFigureAdded(PortalFigure figure)
    {
        if (figure.Parent == Portal)
        {
            if (figure.Parent.currentlyQueryingFigure != null || !figure.Parent.readFigures)
            {
                if (figure.Parent.State < PortalState.Standby) return;
                figure.Parent.FiguresInQueue.Add(figure);
                return;
            }
            DoFigure(figure);
        }
    }

    private void PortalFigureRemoved(PortalFigure figure)
    {
        if (figure.Parent == Portal)
        {
            if (figure.Parent.FiguresInQueue.Contains(figure))
                figure.Parent.FiguresInQueue.Remove(figure);

            if (figure.Parent.currentlyQueryingFigure == figure)
            {
                figure.Parent.currentlyQueryingFigure = null;
                FigureNoLongerBeingQueried(figure);
            }

            if (finishedFigures.Contains(figure))
            {
                finishedFigures.Remove(figure);
                if (alertedFigure == figure)
                    HideAlert();
            }
        }
    }

    private void PortalInputReport(PortalOfPower portal, byte[] data)
    {
        if ((char)data[0] == 'A')
        {
            if (portal.State == PortalState.CommunicatingWithAntenna)
            {
                portal.bootCTS.Cancel();
                portal.State = PortalState.Standby;
            }
        }
        else if ((char)data[0] == 'R')
        {
            if (portal.State == PortalState.SetUpForInterface)
            {
                portal.bootCTS.Cancel();
                portal.State = PortalState.CommunicatingWithAntenna;
            }
            
            foreach (PortalFigure fig in portal.Figures)
                fig.Presence = FigurePresence.NotPresent;
        }
    }
}
