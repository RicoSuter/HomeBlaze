using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Components.Dialogs;
using HomeBlaze.Things;

using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

using System.Reflection;
using System;
using System.Linq;
using System.Threading;

namespace HomeBlaze.Host.Pages.Dashboard
{
    public partial class Index
    {
        private IDisposable? _eventSubscription;

        private Diagram _diagram;

        public bool _isEditMode;

        private HomeBlaze.Things.Dashboard? _selectedDashboard;
        private HomeBlaze.Things.Dashboard[] _dashboards = Array.Empty<HomeBlaze.Things.Dashboard>();

        [Parameter]
        [SupplyParameterFromQuery(Name = "name")]
        public string? Name { get; set; }

        public Index()
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
            _diagram.RegisterModelComponent<WidgetNodeModel, WidgetNode>();
            _diagram.SelectionChanged += node =>
            {
                InvokeAsync(StateHasChanged);
            };

            _diagram.Nodes.Removed += node =>
            {
                if (node is WidgetNodeModel widgetNodeModel && _selectedDashboard != null)
                {
                    _selectedDashboard.Widgets.Remove(widgetNodeModel.Widget);
                    ThingManager.DetectChanges(_selectedDashboard);
                }
            };

            _diagram.KeyDown += OnKeyDown;
        }

        protected override void OnInitialized()
        {
            _eventSubscription = EventManager
                .Subscribe(message =>
                {
                    if (message is ThingRegisteredEvent thingRegisteredEvent &&
                        thingRegisteredEvent.Thing is HomeBlaze.Things.Dashboard)
                    {
                        InvokeAsync(() => RefreshDashboard());
                    }
                    else if (message is ThingUnregisteredEvent thingUnregisteredEvent &&
                        thingUnregisteredEvent.Thing is HomeBlaze.Things.Dashboard)
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

        private void RefreshDashboard(object? sender, LocationChangedEventArgs e)
        {
            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            if (ThingManager != null)
            {
                _dashboards = ThingManager.AllThings.OfType<HomeBlaze.Things.Dashboard>().ToArray();
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
                            var node = new WidgetNodeModel(widget)
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

        public async void AddDashboard(MouseEventArgs args)
        {
            var dashboard = await DashboardDialog.CreateAsync(DialogService);
            if (dashboard != null && ThingManager != null)
            {
                var systemThing = ThingManager.RootThing as SystemThing;
                if (systemThing != null)
                {
                    systemThing.Things.Add(dashboard);
                    ThingManager.DetectChanges(systemThing);
                }
            }
        }

        public async void EditDashboard(MouseEventArgs args)
        {
            if (_selectedDashboard != null)
            {
                await DashboardDialog.EditAsync(DialogService, _selectedDashboard);
                StateHasChanged();
            }
        }

        public void DeleteDashboard(MouseEventArgs args)
        {
            var systemThing = ThingManager?.RootThing as SystemThing;
            if (systemThing != null && _selectedDashboard != null && ThingManager != null)
            {
                systemThing.Things.Remove(_selectedDashboard);
                ThingManager.DetectChanges(systemThing);
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
            if (ThingStorage != null)
            {
                foreach (var model in _diagram
                    .GetSelectedModels()
                    .OfType<WidgetNodeModel>()
                    .ToArray())
                {
                    if (model.Widget.Thing != null)
                    {
                        var clonedThing = ThingStorage.CloneThing(model.Widget.Thing);
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

                _diagram.Nodes.Add(new WidgetNodeModel(widget));
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
                    .OfType<WidgetNodeModel>()
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

        public void DeleteSelectedWidget(MouseEventArgs args)
        {
            if (_diagram != null && _selectedDashboard != null)
            {
                foreach (var model in _diagram
                    .GetSelectedModels()
                    .OfType<WidgetNodeModel>()
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
                    .OfType<WidgetNodeModel>()
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
        }
    }
}
