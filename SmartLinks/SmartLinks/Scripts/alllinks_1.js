var lightBox = function(){
    var show = function () {
        $.post('/SmartLinks/AllData',
        {

        }, function (output) {
            for (var i = 1; i <= output.data.length; i++) {
                var div_str = '<div class="div-title hide">' +
                            '<span class="glyphicon glyphicon-remove span-times delLink"' +
                                'data-link-name="' + output.data[i].LinkName + '" aria-hidden="true" title="Delete"></span>' +
                            '</div>'+
                            '<span class="bg-default-data-transparent" data-name="' + output.data[i].LinkName + '">&nbsp;</span>';
                $('#link' + i).removeClass('bg-default-link').addClass('bg-default-data');
                $('#link' + i).append(div_str);
            }
            $.each(function () {

            })
        });
        
        $('body').on('click', '.bg-default-data-transparent', function () {
            var link_name = $(this).attr('data-name');
            alert(link_name);
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

        $('body').on('click', '.bg-default-link', function () {
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

