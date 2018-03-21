var TechVideo = function () {
    var pageinit = function () {
        $('body').on('mouseenter', '.search-video', function () {
            $('#keywords').removeClass('hide');
            setTimeout(
                "if($('#keywords').val == '') $('#keywords').addClass('hide')"
            , 3000);
        });

        $('body').on('mouseleave', '.search-video', function () {
            if ($('#keywords').val() == '') {
                $('#keywords').addClass('hide');
            }
        });

        $('body').on('click', '.search-img', function () {
            if ($('#keywords').val()) {
                window.location.href = '/TVideoSite/TechnicalVideo?searchkey=' + $('#keywords').val();
            }
        });

        $('body').on('click', '.item-content', function () {
            var vid = $(this).attr("videoid");
            if (vid) {
                window.location.href = '/TVideoSite/TechnicalVideo?activeid=' + vid;
            }
        });

        

        $('body').on('click', '.add-video-btn', function () {
            $("#add-video-modal").modal({ backdrop: 'static' });
        });
        
        $('body').on('click', '.upload-video-upload', function () {

            var subject = $.trim($('#vsubject').val());
            if (subject === '')
            {
                alert('Subject need to be input!')
                return false;
            }

            var vfn = $('#attachmentupload').val();
            var ext = vfn.substr((vfn.lastIndexOf('.') + 1)).toLowerCase();
            if (".mp4,.mp3,.h264,.wmv,.wav,.avi,.flv,.mov,.mkv,.webm,.ogg,.mov,.mpg".indexOf(ext) == -1)
            {
                alert('A video file is necessary!')
                return false;
            }

            $("#add-video-modal").modal('hide');

            $('#uploadvideoform').submit();
        });
        
    };
    return {
        Init: function () {
            pageinit();
        }
    };
}();