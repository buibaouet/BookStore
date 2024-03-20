(function ($) {
    "use strict";
    
    // Dropdown on mouse hover
    $(document).ready(function () {
        function toggleNavbarMethod() {
            if ($(window).width() > 992) {
                $('.navbar .dropdown').on('mouseover', function () {
                    $('.dropdown-toggle', this).trigger('click');
                }).on('mouseout', function () {
                    $('.dropdown-toggle', this).trigger('click').blur();
                });
            } else {
                $('.navbar .dropdown').off('mouseover').off('mouseout');
            }
        }
        toggleNavbarMethod();
        $(window).resize(toggleNavbarMethod);

        $('.register-form #inp-password').on('input', function () {
            let pass = this.value;

            if (pass.length == 0) {
                $(this.nextElementSibling).text('Mật khẩu không được để trống');
            }
            else if (pass.length < 8) {
                $(this.nextElementSibling).text('Mật khẩu chứa tối thiểu 8 ký tự');
            }
            else if (!(pass.match(/[a-zA-Z]/) && pass.match(/[0-9]/) && pass.match(/([!,%,&,@,#,$,^,*,?,_,~])/))) {
                $(this.nextElementSibling).text('Mật khẩu chứa tối thiểu 1 chữ số và 1 ký tự đặc biệt');
            }
            else {
                $(this.nextElementSibling).text('');
            }
        });

        $('.register-form #inp-re-password').on('input', function () {
            let repass = this.value,
                pass = $('.register-form #inp-password').val();                ;

            if (repass !== pass) {
                $(this.nextElementSibling).text('Mật khẩu không khớp');
            }
            else {
                $(this.nextElementSibling).text('');
            }
        });
    });
    
    
    // Back to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 100) {
            $('.back-to-top').fadeIn('slow');
        } else {
            $('.back-to-top').fadeOut('slow');
        }
    });
    $('.back-to-top').click(function () {
        $('html, body').animate({scrollTop: 0}, 1500, 'easeInOutExpo');
        return false;
    });


    // Vendor carousel
    $('.vendor-carousel').owlCarousel({
        loop: true,
        margin: 29,
        nav: false,
        autoplay: true,
        smartSpeed: 1000,
        responsive: {
            0:{
                items:2
            },
            576:{
                items:3
            },
            768:{
                items:4
            },
            992:{
                items:5
            },
            1200:{
                items:6
            }
        }
    });


    // Related carousel
    $('.related-carousel').owlCarousel({
        loop: true,
        margin: 29,
        nav: false,
        autoplay: true,
        smartSpeed: 1000,
        responsive: {
            0:{
                items:1
            },
            576:{
                items:2
            },
            768:{
                items:3
            },
            992:{
                items:4
            }
        }
    });


    // Product Quantity
    $('.quantity button').on('click', function () {
        debugger
        var button = $(this);
        var oldValue = button.parent().parent().find('input').val();
        if (button.hasClass('btn-plus')) {
            var newVal = parseFloat(oldValue) + 1;
        } else {
            if (oldValue > 1) {
                var newVal = parseFloat(oldValue) - 1;
            } else {
                newVal = 1;
            }
        }
        button.parent().parent().find('input').val(newVal);
    });

})(jQuery);

