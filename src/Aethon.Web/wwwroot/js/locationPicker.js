/**
 * locationPicker.js — Google Places Autocomplete integration for LocationPicker.razor
 *
 * Exposes:
 *   window.locationPicker.init(elementId, dotNetRef)
 *   window.locationPicker.destroy(elementId)
 */
window.locationPicker = (function () {
    const instances = {};

    function init(elementId, dotNetRef) {
        const input = document.getElementById(elementId);
        if (!input) return;

        // Guard: Google Maps must be loaded
        if (!window.google || !window.google.maps || !window.google.maps.places) {
            console.warn('locationPicker: Google Maps Places library not loaded for', elementId);
            return;
        }

        // Destroy any existing instance for this element
        destroy(elementId);

        const autocomplete = new google.maps.places.Autocomplete(input, {
            types: ['(cities)'],
            fields: ['address_components', 'geometry', 'name', 'place_id', 'formatted_address']
        });

        const listener = autocomplete.addListener('place_changed', () => {
            const place = autocomplete.getPlace();
            if (!place || !place.geometry) return;

            let city = '';
            let state = '';
            let country = '';
            let countryCode = '';

            for (const component of (place.address_components || [])) {
                const types = component.types;
                if (types.includes('locality') || types.includes('postal_town')) {
                    city = component.long_name;
                } else if (types.includes('administrative_area_level_1')) {
                    state = component.long_name;
                } else if (types.includes('country')) {
                    country = component.long_name;
                    countryCode = component.short_name;
                }
            }

            const lat = place.geometry.location.lat();
            const lng = place.geometry.location.lng();
            const displayText = place.formatted_address || place.name || '';
            const placeId = place.place_id || '';

            dotNetRef.invokeMethodAsync(
                'OnPlaceSelected',
                city, state, country, countryCode, lat, lng, placeId, displayText
            );
        });

        instances[elementId] = { autocomplete, listener };
    }

    function destroy(elementId) {
        const instance = instances[elementId];
        if (!instance) return;
        google.maps.event.removeListener(instance.listener);
        delete instances[elementId];
    }

    return { init, destroy };
})();
