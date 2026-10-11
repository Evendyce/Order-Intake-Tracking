// Apply the stored palette before Angular renders to avoid a light/dark flash.
(() => {
  let theme = 'dark';
  try {
    if (localStorage.getItem('vg-theme') === 'light') theme = 'light';
  } catch {
    // A blocked preference store falls back to VoidGlass's default dark palette.
  }
  document.documentElement.dataset.theme = theme;
  document.documentElement.style.colorScheme = theme;
})();
