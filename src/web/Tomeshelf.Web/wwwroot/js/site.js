// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    const submenuToggles = document.querySelectorAll(".dropdown-submenu > .dropdown-toggle");
    submenuToggles.forEach((toggle) => {
        toggle.addEventListener("click", (event) => {
            event.preventDefault();
            event.stopPropagation();

            const submenu = toggle.nextElementSibling;
            if (!submenu) {
                return;
            }

            submenu.classList.toggle("show");
            const isOpen = submenu.classList.contains("show");
            toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
        });
    });

    document.querySelectorAll(".dropdown").forEach((dropdown) => {
        dropdown.addEventListener("hide.bs.dropdown", () => {
            dropdown.querySelectorAll(".dropdown-menu.show").forEach((menu) => {
                menu.classList.remove("show");
            });
            dropdown.querySelectorAll(".dropdown-submenu > .dropdown-toggle").forEach((toggle) => {
                toggle.setAttribute("aria-expanded", "false");
            });
        });
    });
});
