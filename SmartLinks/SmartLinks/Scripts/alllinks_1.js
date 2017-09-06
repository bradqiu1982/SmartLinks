var lightBox = function(){
    var show = function () {
        $.post('/SmartLinks/AllData',
        {

        }, function (output) {
            for (var i = 1; i <= output.data.length; i++) {
                var link_name = output.data[i - 1].LinkName;
                if (output.data[i - 1].LinkName.length > 10) {
                    link_name = output.data[i - 1].LinkName.substring(0, 10) + '...';
                }
                var div_str = '<div class="div-title hide">' +
                            '<span class="glyphicon glyphicon-remove span-times delLink"' +
                                'data-link-name="' + output.data[i-1].LinkName + '" aria-hidden="true" title="Delete"></span>' +
                            '</div>'+
                            '<span class="bg-default-data-transparent" data-name="' + output.data[i - 1].LinkName + '">'+
                            '<div class="div-link-name"><span class="span-link-name">' + link_name + '</span></div></span>';
                $('#link' + i).append(div_str);
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
                    '<span class="bg-default-link-transparent">'+
                        '<div class="div-link-name">'+
                            '<span class="span-link-name"></span>'+
                        '</div>'+
                    '</span>'
                );
            })
            $('.bg-default-etc').append(
                '<span class="bg-default-link-transparent">' +
                    '<div class="div-link-name">' +
                        '<span class="span-link-name"></span>' +
                    '</div>' +
                '</span>'
            );
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

        $('body').on('click', '.bg-default-link, .bg-default-link-rect', function () {
            $('#link_name').val('');
            $('#link').val('');
            $('#comment').val('');
            $('#imgurl').val('');
            $('.addLink').removeClass('hide');
        });

        $('body').on('click', '#popup_cancel', function () {
            $('.addLink').addClass('hide');
        })

        $('body').on('click', '#popup_add_Link', function () {
            var link_name = $('#link_name').val();
            var link = $('#link').val();
            var comment = $('#comment').val();
            var image_url = $('#imgurl').val();
            if (!link_name || !link) {
                alert('Please input LinkName and Link !');
                return false;
            }

            $.post('/SmartLinks/AddCustomLink',
            {
                link: link,
                link_name: link_name,
                comment: comment,
                image_url: image_url
            }, function (output) {
                console.log(output);
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

