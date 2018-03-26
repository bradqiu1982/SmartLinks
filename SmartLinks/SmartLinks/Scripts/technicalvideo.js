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
            if (vfn === '') {
                alert('Video file need to be selected!')
                return false;
            }
            var ext = vfn.substr((vfn.lastIndexOf('.') + 1)).toLowerCase();
            if (".mp4,.mp3,.h264,.wmv,.wav,.avi,.flv,.mov,.mkv,.webm,.ogg,.mov,.mpg".indexOf(ext) == -1)
            {
                alert('Video file need to be selected!')
                return false;
            }

            $("#add-video-modal").modal('hide');

            $('#uploadvideoform').submit();
        });
        
        $('body').on('click', '.add-test-btn', function () {
            $("#add-test-modal").modal({ backdrop: 'static' });
        });

        $('body').on('click', '.upload-test-upload', function () {

            var excelfile = $("#attachmentupload1").val();
            if (excelfile === '')
            {
                alert('An excel file which contains test need to be selected!')
                return false;
            }

            var ext = excelfile.substr((excelfile.lastIndexOf('.') + 1)).toLowerCase();
            if (".xlsx,.xls,.xlsm,.csv,.ods,.xml".indexOf(ext) == -1) {
                alert('An excel file which contains test need to be selected!')
                return false;
            }

            var giftfile = $("#attachmentupload2").val();
            if (giftfile != '')
            {
                ext = giftfile.substr((giftfile.lastIndexOf('.') + 1)).toLowerCase();
                if (".jpg,.jpeg,.png,.bmp".indexOf(ext) == -1) {
                    alert('An excel file which contains test need to be selected!')
                    return false;
                }
            }
            $("#add-test-modal").modal('hide');
            $('#uploadtestform').submit();
        });

        
    };
    return {
        Init: function () {
            pageinit();
        }
    };
}();