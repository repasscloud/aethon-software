/**
 * locationPicker.js — Google Places Autocomplete integration for LocationPicker.razor
 *
 * Exposes:
 *   window.locationPicker.init(elementId, dotNetRef, initialValue)
 *   window.locationPicker.destroy(elementId)
 *   window.locationPicker.setValue(elementId, value)
 */
window.locationPicker = (function () {
    const instances = {};
    const MAX_RETRIES = 10;

    function init(elementId, dotNetRef, initialValue, retryCount) {
        retryCount = retryCount || 0;

        const input = document.getElementById(elementId);
        if (!input) return;

        // Set initial value so the field shows existing data without Blazor binding
        if (initialValue) input.value = initialValue;

        // Guard: Google Maps must be loaded — retry up to MAX_RETRIES times
        if (!window.google || !window.google.maps || !window.google.maps.places) {
            if (retryCount < MAX_RETRIES) {
                setTimeout(() => init(elementId, dotNetRef, initialValue, retryCount + 1), 300);
            } else {
                console.warn('locationPicker: Google Maps Places library not available for', elementId);
            }
            return;
        }

        // Destroy any existing instance for this element
        destroy(elementId);

        const autocomplete = new google.maps.places.Autocomplete(input, {
            types: ['geocode'],
            fields: ['address_components', 'geometry', 'name', 'place_id', 'formatted_address']
        });

        // When user selects a place from the dropdown
        const placeListener = autocomplete.addListener('place_changed', () => {
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

        // When user manually types and leaves the field without selecting a place
        const blurHandler = () => {
            dotNetRef.invokeMethodAsync('OnTextChanged', input.value || '');
        };
        input.addEventListener('blur', blurHandler);

        instances[elementId] = { autocomplete, placeListener, blurHandler };
    }

    function destroy(elementId) {
        const instance = instances[elementId];
        if (!instance) return;
        if (instance.placeListener) google.maps.event.removeListener(instance.placeListener);
        const input = document.getElementById(elementId);
        if (input && instance.blurHandler) input.removeEventListener('blur', instance.blurHandler);
        delete instances[elementId];
    }

    function setValue(elementId, value) {
        const input = document.getElementById(elementId);
        if (input) input.value = value || '';
    }

    return { init, destroy, setValue };
})();
