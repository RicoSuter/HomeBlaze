window.ScrollIntoView = function (elementId, left) {
    const element = document.getElementById(elementId);
    element.scrollTo({ left: left, behavior: 'smooth' });
};
window.GetWindowHeight = function () { return window.outerHeight; };
window.GetWindowWidth = function () { return window.outerWidth; };