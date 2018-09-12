var lightBox = function () {
    var show = function () {
        //check ie8
        var ie8_flg = !$.support.leadingWhitespace;
        if (ie8_flg) {
            $('.logo').removeClass('logo').addClass('logo-ie8');
            $('.img-computer').removeClass('img-computer').addClass('img-computer-ie8');
        }

        $('.carousel').carousel({ interval: false });
        var default_imgs = new Array();
        var default_imgs_ie8 = new Array();
        var default_links_ie8 = new Array();
        default_imgs = ['./Content/images/peace.png', './Content/images/data.png', './Content/images/daisy.png'
            , './Content/images/clover.png', './Content/images/scrap.png', './Content/images/component.png'
            , './Content/images/video.png', './Content/images/lego.png', './Content/images/Assets.png', './Content/images/distribution.png'
            , './Content/images/scraprate.png', './Content/images/HPUTREND.png', './Content/images/snteststatus.png', './Content/images/VCSEL.png', './Content/images/CPK.png'];
        default_imgs_ie8 = ['./Content/images/ie8/peace.png', './Content/images/ie8/data.png', './Content/images/ie8/daisy.png'
            , './Content/images/ie8/clover.png', './Content/images/ie8/scrap.png', './Content/images/ie8/component.png'
            , './Content/images/ie8/video.png', './Content/images/ie8/lego.png', './Content/images/ie8/Assets.png', './Content/images/ie8/distribution.png'
            , './Content/images/ie8/scraprate.png', './Content/images/ie8/HPUTREND.png', './Content/images/ie8/snteststatus.png', './Content/images/ie8/VCSEL.png', './Content/images/ie8/CPK.png'];
        default_links_ie8 = ['AGILE', 'BR', 'DOMINO', 'ERP', 'FA', 'TRACEVIEW', 'DAISY', 'OA', 'CLOVER', 'SCRAP'
            , 'COMPONENT', 'VIDEO', 'LEGO', 'ASSETS', 'DISTRIBUTION', 'SCRAPRATE', 'HPUTREND', 'SNTESTSTATUS', 'VCSEL','CPK'];

        $.post('/SmartLinks/AllData',
        {

        }, function (output) {
            var link_name = '';
            var logo = '';
            for (var i = 1; i <= output.data.length; i++) {
                link_name = output.data[i - 1].LinkName;
                if (output.data[i - 1].LinkName.length > 15) {
                    link_name = output.data[i - 1].LinkName.substring(0, 15) + '..';
                }
                var div_str = '<div class="div-title hide">' +
                            '<span class="glyphicon glyphicon-remove span-times delLink"' +
                                'data-link-name="' + output.data[i - 1].LinkName + '" aria-hidden="true" title="Delete"></span>' +
                            '</div>' +
                            '<span class="bg-default-data-transparent" ' +
                            'data-name="' + output.data[i - 1].LinkName + '">' +
                            '<div class="div-link-name"><span class="span-link-name">' + link_name + '</span></div></span>';
                $('#link' + i).append(div_str);
                logo = output.data[i - 1].Logo;
                if (logo) {
                    if (ie8_flg) {
                        //if brower is ie8 or blow and logo in default array, change logo url with suffix -ie8. 
                        var logo_array = logo.split('/');
                        var logo_name = logo_array[logo_array.length - 1].split('-')[0];
                        if ($.inArray(logo_name.toUpperCase(), default_links_ie8) !== -1) {
                            logo = './Content/images/ie8/' + logo_name + '.png';
                        }
                    }
                }
                else {
                    //if brower is ie8, use ie8 default logo. 
                    logo = (ie8_flg) ? default_imgs_ie8[(i - 1) % 4] : default_imgs[(i - 1) % 4];
                }
                $('#link' + i).attr('style', 'background-image: url(' + logo + ');');
                if ($('#link' + i).hasClass('bg-default-link')) {
                    $('#link' + i).removeClass('bg-default-link').addClass('bg-default-data');
                }
                if ($('#link' + i).hasClass('bg-default-link-rect')) {
                    $('#link' + i).removeClass('bg-default-link-rect').addClass('bg-default-data-rect');
                    $('#link' + i).children('span').removeClass('bg-default-data-transparent').addClass('bg-default-data-transparent-rect');
                }
            }
            $('.bg-default-link').each(function () {
                $(this).append(
                    '<span class="bg-default-link-transparent">' +
                        '<div class="div-link-name">' +
                            '<span class="span-link-name"></span>' +
                        '</div>' +
                    '</span>'
                );
            });
            $('.bg-default-etc').append(
                '<span class="bg-default-link-transparent">' +
                    '<div class="div-link-name">' +
                        '<span class="span-link-name"></span>' +
                    '</div>' +
                '</span>'
            );

            if (ie8_flg) {
                $('.bg-default-link').removeClass('bg-default-link').addClass('bg-default-link-ie8');
                $('.bg-default-etc').removeClass('bg-default-etc').addClass('bg-default-etc-ie8');
            }
        });

        $('body').on('click', '.bg-default-data-transparent, .bg-default-data-transparent-rect', function () {
            var link_name = $(this).attr('data-name');
            window.location.href = '/SmartLinks/RedirectToLink?linkname=' + link_name;
        })

        $('body').on('mouseenter', '.cus_link', function () {
            if ($(this).children('.div-title').hasClass('hide')) {
                $(this).children('.div-title').removeClass('hide');
            }
        })

        $('body').on('mouseleave', '.cus_link', function () {
            if (!$(this).children('.div-title').hasClass('hide')) {
                $(this).children('.div-title').addClass('hide');
            }
        })

        $('body').on('click', '.bg-default-link, .bg-default-link-rect, .bg-default-link-ie8, .bg-default-etc-ie8', function () {
            $('#link_name').val('');
            $('#link').val('');
            $('#comment').val('');
            $('#imgurl').val('');
            //check ie8
            var ie8_flg = !$.support.leadingWhitespace;
            ie8_flg = true;
            if (ie8_flg) {
                $('.input-placeholder').each(function () {
                    $(this).val($(this).attr('placeholder'));
                })
            }

            $('.addLink').removeClass('hide');
        });

        $('body').on('focus', '.input-placeholder', function () {
            if ($(this).attr('placeholder') == $(this).val()) {
                $(this).val('');
            }
        })

        $('body').on('blur', '.input-placeholder', function () {
            if ($(this).val() == '') {
                $(this).val($(this).attr('placeholder'));
            }
        })

        $('body').on('click', '#popup_cancel', function () {
            $('.addLink').addClass('hide');
        })

        $('body').on('click', '#popup_add_Link', function () {
            var link_name = $('#link_name').val();
            var link = $('#link').val();
            var comment = $('#comment').val();
            var image_url = $('#imgurl').val();
            if (!link_name || !link || link_name == 'Link Name' || link == "Link") {
                alert('Please input LinkName and Link !');
                return false;
            }

            if (comment == "Comment") {
                comment = '';
            }

            $.post('/SmartLinks/AddCustomLink',
            {
                link: link,
                link_name: link_name,
                comment: comment,
                image_url: image_url
            }, function (output) {
                if (output.success) {
                    $('.addLink').modal('hide');
                    window.location.reload();
                }
                else {
                    window.alert("fail to add link");
                }
            });
        });

        $('body').on('click', '.delLink', function () {
            if (!confirm('Do you really want to delete this this link ?')) {
                return false;
            }
            var link_name = $(this).attr('data-link-name');
            $.post('/SmartLinks/RemoveCustomLink',
            {
                link_name: link_name
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
                else {
                    alert('Fail to delete this link !');
                }
            })
        })
    }
    return {
        init: function () {
            show();
        }
    };
}();

