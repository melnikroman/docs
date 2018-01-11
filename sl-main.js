function hideExtraMenu() {
    $('[data-js="master-navigation-extra-menu-content"]').removeClass('is-opened');
    $('[data-js="master-navigation-extra-menu-content"]').attr('onclick', 'showExtraMenu(this);');
    $('[data-js="header-extra-menu-container"]').html('');
}


function showExtraMenu(obj) {
    hideExtraMenu();
    $(obj).attr('onclick', 'hideExtraMenu(this);');
    $(obj).addClass('is-opened');
    $('[data-js="header-extra-menu-container"]').html($(obj).html());
}



$(document).ready(function () {



    $('[data-js="m-master-left-menu-expand"]').click(function () {
        $(this).toggleClass('folded');
        if ($(this).hasClass('folded')) {
            document.cookie = "leftMenuStatus=folded; path=/; expires=Mon, 13-Jan-2020 00:00:00 GMT";
        } else {
            document.cookie = "leftMenuStatus=; path=/; expires=Mon, 13-Jan-2020 00:00:00 GMT";
        }
    });




    $("[data-js='site-hierarchy-menu-button']").click(function () {
        var clickedElement = this;
        var parent = $(this).parent('.parent');
        if (parent.has('.child-group').length) {
            parent.toggleClass("is-expanded");
        }
        if (parent.hasClass('is-expanded')) {
            document.cookie = parent.attr('id') + "=is-expanded; path=/; expires=Mon, 13-Jan-2020 00:00:00 GMT";
        } else {
            document.cookie = parent.attr('id') + "=; path=/; expires=Mon, 13-Jan-2020 00:00:00 GMT"; 
        }
    });

    $('[data-js="expand-documents-list"]').click(function () {
        $(this).toggleClass('is-expanded');
        $('[data-js="documents-list"]').toggleClass('is-expanded');
        $('[data-js="documents-navigation"]').toggleClass('is-collapse');
    });

    var owl = $('[data-js="site-slider"]');
    if (owl.children().length > 1) {
        owl.owlCarousel({
            loop: true,
            nav: true,
            navText: false,
            navigation: true,
            navigationText: false,
            mouseDrag: false,
            touchDrag: false,
            items: 1
        });
    } else {
        $('[data-js="site-slider"]').show();
    }

    $('[data-js="link-hover-for-submenu"]').hover(function () {
        $(this).prev().css('display', 'block');
    });

    $(function () {
        $('[data-js="popup-slider-video"]').magnificPopup({
            type: 'inline',
            preloader: false,
            arrowMarkup: '<button title="%title%" type="button" class="mfp-arrow test mfp-arrow-%dir%"></button>', // markup of an arrow button
            modal: true
        });
    });



    $('[data-js="informer-birthdays-list"]').owlCarousel({
        items: 1,
        navigation: true,
        navigationText: false,
        pagination: false,
        responsive: true,
        mouseDrag: false,
        touchDrag: false
    });
});