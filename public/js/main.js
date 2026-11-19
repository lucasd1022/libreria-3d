// Pequeñas mejoras progresivas: la tienda funciona igual sin JavaScript.
document.addEventListener("change", (e) => {
  if (e.target.matches("[data-autosubmit]")) e.target.form.submit();
});

document.addEventListener("submit", (e) => {
  const mensaje = e.target.dataset.confirmar;
  if (mensaje && !confirm(mensaje)) e.preventDefault();
});
