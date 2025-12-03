// mapsUtility.js

/**
 * Function to initialize a Leaflet map for a given modal and map container selector
 * @param {string} modalSelector - Selector for the modal (e.g., '#AddCityModal')
 * @param {string} mapSelector - Selector for the map container (e.g., '#map')
 * @param {string|null} existingArea - Optional WKT or GeoJSON string for existing geometry to load (e.g., for editing)
 * @param {Object} drawOptions - Optional configuration object for draw controls
 *                                {
 *                                    allowPolygon: true/false,
 *                                    allowPoint: true/false,
 *                                    allowRectangle: true/false,
 *                                    ...
 *                                }
 */
function initializeMap(modalSelector, mapSelector, existingArea = null, drawOptions = {}, icon = '/MapIcons/defaultMarker.png') {
    //if (icon == null) {
    //    icon = '/MapIcons/defaultMarker.png';
    //}
    $(modalSelector).on('shown.bs.modal', function () {
        setTimeout(function () {
            if ($(mapSelector).length) {
                // Set initial view to Sabzevar city coordinates
                var map = L.map(mapSelector.replace('#', '')).setView([36.2126, 57.6770], 13);


                // Add a tile layer to the map
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                }).addTo(map);

                // Initialize a feature group to store drawn items
                var drawnItems = new L.FeatureGroup();
                map.addLayer(drawnItems);

                // Load existing geometry (if available)
                if (existingArea) {
                    try {
                        var geoJson;

                        // Check if existingArea is in WKT format
                        if (existingArea.trim().startsWith("POLYGON") ||
                            existingArea.trim().startsWith("POINT") ||
                            existingArea.trim().startsWith("LINESTRING")) {

                            // Convert WKT to GeoJSON using Terraformer WKT parser
                            geoJson = Terraformer.WKT.parse(existingArea);
                        } else {
                            // Assume it is GeoJSON format
                            geoJson = JSON.parse(existingArea);
                        }

                        // Create a GeoJSON layer and add it to drawnItems feature group
                        L.geoJSON(geoJson, {
                            onEachFeature: function (feature, layer) {
                                drawnItems.addLayer(layer);
                            }
                        });

                        // Fit map bounds to the loaded layer
                        map.fitBounds(drawnItems.getBounds());
                    } catch (error) {
                        console.error("Error parsing existing area GeoJSON or WKT:", error);
                    }
                }

                // Define draw options based on the input configuration
                var drawControlOptions = {
                    position: 'topright',
                    edit: {
                        featureGroup: drawnItems,
                        remove: true
                    },
                    draw: {
                        polygon: drawOptions.allowPolygon ? {
                            allowIntersection: false,
                            showArea: true,
                            drawError: {
                                color: '#e1e100',
                                message: "<strong>Oh snap!</strong> you can't draw that!"
                            },
                            shapeOptions: {
                                color: '#bada55'
                            }
                        } : false,
                        marker: drawOptions.allowPoint ? {
                            //icon: L.icon({
                            //    iconUrl: icon,
                            //    //shadowUrl: 'https://leafletjs.com/examples/custom-icons/leaf-shadow.png',
                            //    iconSize: [38, 95],
                            //    shadowSize: [50, 64],
                            //    iconAnchor: [22, 94],
                            //    shadowAnchor: [4, 62],
                            //    popupAnchor: [-3, -76]
                            //})
                        } : false,
                        rectangle: drawOptions.allowRectangle ? {
                            shapeOptions: {
                                color: '#ff7800'
                            }
                        } : false,
                        circle: false,
                        circlemarker: false,
                        polyline: false
                    }
                };

                // Initialize draw control and add to the map
                var drawControl = new L.Control.Draw(drawControlOptions);
                map.addControl(drawControl);

                // Handle draw:created event to add new geometry
                map.on(L.Draw.Event.CREATED, function (event) {
                    drawnItems.clearLayers(); // Only allow one geometry at a time
                    var layer = event.layer;
                    drawnItems.addLayer(layer);

                    // Update the hidden input with the new geometry
                    var geoJson = layer.toGeoJSON().geometry;
                    $(modalSelector + ' #polygon').val(JSON.stringify(geoJson));
                });

                // Handle draw:edited event to update the hidden field
                map.on(L.Draw.Event.EDITED, function (event) {
                    var layers = event.layers;
                    layers.eachLayer(function (layer) {
                        var geoJson = layer.toGeoJSON().geometry;
                        $(modalSelector + ' #polygon').val(JSON.stringify(geoJson));
                    });
                });

                // Handle events to enable/disable panning when editing
                map.on('draw:editstart', function () {
                    // Disable map dragging when editing starts
                    map.dragging.disable();
                });

                map.on('draw:editstop', function () {
                    // Enable map dragging when editing stops
                    map.dragging.enable();
                });

                // Invalidate map size to ensure proper rendering
                map.invalidateSize();
            }
        }, 10); // Adding a small delay to ensure the modal is fully displayed
    });
}

