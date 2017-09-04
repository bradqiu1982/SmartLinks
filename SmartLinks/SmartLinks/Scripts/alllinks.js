var lightBox = function(){
	var show = function(){
        $('[data-toggle="popover"]').popover();
        $('.carousel').carousel({
            interval: false
        });
        $('body').on('click', '.span-times', function() {
            $(this).parent().parent().hide();
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


	}
    return {
        init: function () {
            show();
        }
    };
}();

