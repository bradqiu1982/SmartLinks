var lightBox = function(){
	var show = function(){
        $('[data-toggle="popover"]').popover();
        $('.carousel').carousel({
            interval: false
        });

        $('body').on('click', '.img_default', function () {
            $('#link_name').val('');
            $('#link').val('');
            $('#comment').val('');
            $('#imgurl').val('');
            $('#modal_add_Link').attr('disabled', 'disabled');
            $('.addLink').modal('show');
        });

        $('body').on('click', '#modal_add_Link', function () {
            var link_name = $('#link_name').val();
            var link = $('#link').val();
            var comment = $('#comment').val();

            var image_url = $('#imgurl').val();
            if (!image_url)
            {
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

        $('body').on('mouseenter', '.thumbnail', function () {
            if ($(this).children('.div-title').hasClass('hide')) {
                $(this).children('.div-title').removeClass('hide');
            }
        })

        $('body').on('mouseleave', '.thumbnail', function () {
            if (!$(this).children('.div-title').hasClass('hide')) {
                $(this).children('.div-title').addClass('hide');
            }
        })

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

