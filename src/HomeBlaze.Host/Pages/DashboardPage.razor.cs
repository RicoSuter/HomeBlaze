using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Components.Dialogs;
using HomeBlaze.Things;

using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

using System.Reflection;
using System;
using System.Linq;
using System.Threading;
using Microsoft.JSInterop;
using Namotion.Reflection;
using HomeBlaze.Messages;

namespace HomeBlaze.Host.Pages
{
    public partial class DashboardPage
    {
        private IDisposable? _eventSubscription;

        private Diagram _diagram;

        public bool _isEditMode;

        private Dashboard? _selectedDashboard;
        private Dashboard[] _dashboards = Array.Empty<Dashboard>();

        private Timer? _autoScaleTimer;
        private decimal _windowWidth;
        private decimal _windowHeight;

        [Parameter]
        [SupplyParameterFromQuery(Name = "name")]
        public string? Name { get; set; }

        public DashboardPage()
        {
            var options = new DiagramOptions
            {
                DeleteKey = "Delete",

                AllowPanning = false,
                AllowMultiSelection = false,

                GridSize = 25,
                Zoom = new DiagramZoomOptions
                {
                    Enabled = false
                }
            };

            _diagram = new Diagram(options);
            _diagram.RegisterModelComponent<DashboardWidgetNodeModel, DashboardWidgetNode>();
            _diagram.SelectionChanged += node =>
            {
                InvokeAsync(StateHasChanged);
            };

            _diagram.Nodes.Removed += node =>
            {
                if (node is DashboardWidgetNodeModel widgetNodeModel && _selectedDashboard != null)
                {
                    _selectedDashboard.Widgets.Remove(widgetNodeModel.Widget);
                    ThingManager.DetectChanges(_selectedDashboard);
                }
            };

            _diagram.KeyDown += OnKeyDown;
        }

        protected override void OnInitialized()
        {
            _autoScaleTimer = new Timer(OnAutoScaleTimer!, null, 1000, 1000);
            _eventSubscription = EventManager
                .Subscribe(message =>
                {
                    if (message is ThingRegisteredEvent thingRegisteredEvent &&
                        thingRegisteredEvent.Thing is Dashboard)
                    {
                        InvokeAsync(() => RefreshDashboard());
                    }
                    else if (message is ThingUnregisteredEvent thingUnregisteredEvent &&
                        thingUnregisteredEvent.Thing is Dashboard)
                    {
                        InvokeAsync(() => RefreshDashboard());
                    }
                    else if (message is ThingStateChangedEvent)
                    {
                        InvokeAsync(() =>
                        {
                            // TODO: Can we optimize this and only refresh the involved nodes?
                            // Remove this and the widgets should listen for messages they need and rerender internally!
                            StateHasChanged();
                           
                            _diagram.Refresh();
                            foreach (var node in _diagram.Nodes)
                            {
                                node.Refresh();
                            }
                        });
                    }
                });

            Navigation.LocationChanged += RefreshDashboard;
            RefreshDashboard();
        }

        private async void OnAutoScaleTimer(object o)
        {
            try
            {
                // TODO: This does not yet work perfectly
                if (_selectedDashboard?.UseAutoScale == true &&
                    JsRuntime.TryGetPropertyValue("IsInitialized", true))
                {
                    _windowWidth = await JsRuntime.InvokeAsync<decimal>("GetWindowWidth");
                    _windowHeight = await JsRuntime.InvokeAsync<decimal>("GetWindowHeight");

                    await InvokeAsync(StateHasChanged);
                }
            }
            catch
            {
            }
        }

        private void RefreshDashboard(object? sender, LocationChangedEventArgs e)
        {
            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            if (ThingManager != null)
            {
                _dashboards = ThingManager.AllThings.OfType<Dashboard>().ToArray();
                _selectedDashboard = null;

                var selectedDashboard = _dashboards
                    .FirstOrDefault(d => d.Name == Name) ?? _dashboards.FirstOrDefault();

                if (selectedDashboard != null && 
                    selectedDashboard != _selectedDashboard)
                {
                    _diagram?.Batch(() =>
                    {
                        _diagram?.Nodes.Clear();

                        foreach (var widget in selectedDashboard.Widgets)
                        {
                            var node = new DashboardWidgetNodeModel(widget)
                            {
                                Locked = !_isEditMode
                            };

                            _diagram?.Nodes.Add(node);
                            node.Refresh();
                        }
                    });

                    _selectedDashboard = selectedDashboard;
                    _diagram?.Refresh();
                }
            }

            StateHasChanged();
        }

        public async void ToggleEditMode(MouseEventArgs args)
        {
            _isEditMode = !_isEditMode;

            _diagram?.Batch(() =>
            {
                foreach (var node in _diagram.Nodes)
                {
                    node.Locked = !_isEditMode;
                    node.Refresh();
                }
            });

            if (!_isEditMode)
            {
                await ThingManager.WriteConfigurationAsync(CancellationToken.None);
            }
        }

        public async void AddDashboard()
        {
            if (ThingManager.RootThing is IGroupThing rootThing)
            {
                var thing = await ThingSetupDialog.AddThingAsync(DialogService, t => t == typeof(Dashboard));
                if (thing != null)
                {
                    rootThing.AddThing(thing);

                    await ThingManager.WriteConfigurationAsync(CancellationToken.None);
                    ThingManager.DetectChanges(rootThing);
                }
            }          
        }

        public async void EditDashboard()
        {
            if (_selectedDashboard != null)
            {
                var hasChanged = await ThingSetupDialog.EditThingAsync(DialogService, _selectedDashboard);
                if (hasChanged)
                {
                    await ThingManager.WriteConfigurationAsync(CancellationToken.None);
                    ThingManager.DetectChanges(_selectedDashboard);
                }
            }
        }

        public async void DeleteDashboard(MouseEventArgs args)
        {
            var delete = await DialogService.ShowMessageBox("Delete Dashboard", "Do you really want to delete this Dashboard?", "Delete", "No");
            if (delete == true && _selectedDashboard != null && ThingManager?.RootThing != null)
            {
                ThingManager.RootThing.RemoveThing(_selectedDashboard);
                ThingManager.DetectChanges(ThingManager.RootThing);
            }
        }

        public async void AddWidget(MouseEventArgs args)
        {
            var thing = await ThingSetupDialog.AddThingAsync(DialogService, t => t.GetCustomAttribute<ThingWidgetAttribute>() != null);
            if (thing != null)
            {
                AddThingWidgetToDashboard(thing);
            }
        }

        public void CloneSelectedWidget(MouseEventArgs args)
        {
            if (ThingSerializer != null)
            {
                foreach (var model in _diagram
                    .GetSelectedModels()
                    .OfType<DashboardWidgetNodeModel>()
                    .ToArray())
                {
                    if (model.Widget.Thing != null)
                    {
                        var clonedThing = ThingSerializer.CloneThing(model.Widget.Thing);
                        AddThingWidgetToDashboard(clonedThing);
                    }
                }

                _diagram.Refresh();
            }
        }

        private void AddThingWidgetToDashboard(IThing? thing)
        {
            if (_diagram != null && _selectedDashboard != null)
            {
                var widget = new Widget { Thing = thing };

                _diagram.Nodes.Add(new DashboardWidgetNodeModel(widget));
                _selectedDashboard.Widgets.Add(widget);

                ThingManager.DetectChanges(_selectedDashboard);
            }
        }

        public void MoveSelectedWidgetToTop(MouseEventArgs args)
        {
            if (_diagram != null && _selectedDashboard != null)
            {
                foreach (var model in _diagram
                    .GetSelectedModels()
                    .OfType<DashboardWidgetNodeModel>()
                    .ToArray())
                {
                    _selectedDashboard.Widgets.Remove(model.Widget);
                    _selectedDashboard.Widgets.Add(model.Widget);

                    _diagram.Nodes.Remove(model);
                    _diagram.Nodes.Add(model);

                    model.Refresh();
                }

                _diagram.Refresh();
            }
        }

        public async void DeleteSelectedWidget(MouseEventArgs args)
        {
            var delete = await DialogService.ShowMessageBox("Delete Widget", "Do you really want to delete this Widget?", "Delete", "No");
            if (delete == true && _diagram != null && _selectedDashboard != null)
            {
                foreach (var model in _diagram
                    .GetSelectedModels()
                    .OfType<DashboardWidgetNodeModel>()
                    .ToArray())
                {
                    _selectedDashboard.Widgets.Remove(model.Widget);
                    _diagram.Nodes.Remove(model);

                    model.Refresh();
                }

                _diagram.Refresh();
                ThingManager.DetectChanges(_selectedDashboard);
            }
        }

        public void OnKeyDown(KeyboardEventArgs args)
        {
            if ((args.AltKey || args.CtrlKey || args.ShiftKey) &&
                args.Code != "ArrowLeft" && args.Code != "ArrowRight" &&
                args.Code != "ArrowUp" && args.Code != "ArrowDown")
            {
                return;
            }

            if (_diagram != null && _selectedDashboard != null)
            {
                foreach (var model in _diagram
                    .GetSelectedModels()
                    .OfType<DashboardWidgetNodeModel>()
                    .ToArray())
                {
                    if (args.Code == "ArrowLeft")
                    {
                        model.Position = new Point(model.Position.X - 5, model.Position.Y);
                    }
                    else if (args.Code == "ArrowRight")
                    {
                        model.Position = new Point(model.Position.X + 5, model.Position.Y);
                    }
                    else if (args.Code == "ArrowUp")
                    {
                        model.Position = new Point(model.Position.X, model.Position.Y - 5);
                    }
                    else if (args.Code == "ArrowDown")
                    {
                        model.Position = new Point(model.Position.X, model.Position.Y + 5);
                    }

                    model.UpdatePosition();
                    model.Refresh();
                }

                _diagram.Refresh();
            }
        }

        public void Dispose()
        {
            Navigation.LocationChanged -= RefreshDashboard;
            _eventSubscription?.Dispose();
            _autoScaleTimer?.Dispose();
        }
    }
}
