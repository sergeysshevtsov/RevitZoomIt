What Does "Zoom to Selection" Mean in Revit?
In Revit, "Zoom to Selection" means adjusting the view so that all selected elements are centered and fit inside the visible window, making them easy to see and work with.

When you activate a zoom command (like from the plugin we just built), Revit:
1. Calculates the bounding box — the smallest 3D box that contains all selected elements.
2. Centers the view on this box.
3. Zooms in or out so the box fits the screen — or more/less depending on the zoom percentage.

Zoom Percentage Interpretation (in your plugin)
- 100% → Fit selected elements tightly to the screen.
- 50% → Zoom in (selected elements fill more of the screen).
- 200% → Zoom out (more space around selected elements).
- The plugin accepts values like 25, 75, 125, etc., interpreted as a percentage.
