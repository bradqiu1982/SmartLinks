var Wafer = function(){
    var show = function(){
       $('body').on('keypress', '#marks', function(e){
            if(e.keyCode == 13){
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function(i, val){
                    if(val != ""){
                        if(arr_count[val]){
                            alert(val+" has already existed.");
                            arr_count[val]++;
                        }
                        else{
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
       })

       $('body').on('click', '#btn-marks-submit', function(){
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function(i, val){
                if(val != ""){
                    if(arr_count[val]){
                        alert(val+" has already existed.");
                        arr_count[val]++;
                    }
                    else{
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            $('#marks').val(cur_marks.join('\n'));
            alert(JSON.stringify(cur_marks));

            // $.post('/', 
            // {
            //     marks: JSON.stringify(cur_marks)
            // }, function(output){

            // })
       })

       $('body').on('click', '.op-setting', function(){
            $('#modal-op-setting').modal('show');
       })

       $('body').on('click', '#m-confirm', function(){
            add_wafer();
       })

        $('body').on('keypress', '#wafer-no', function(e){
            if(e.keyCode == 13){
                add_wafer();
            }
        })

       var add_wafer = function(){
            var wafer_no = $.trim($('#wafer-no').val());
            if(wafer_no == ""){
                return false;
            }
            var flg = false;
            $('.m-wafer').each(function () {
                if ($(this).attr('data-val') == wafer_no) {
                    flg = true;
                    return;
                }
            });
            if (!flg) {
                $('.m-wafer-list').append(
                    '<div class="m-wafer" data-val="' + wafer_no + '" data-name="">' +
                    '<span>' + wafer_no + '</span>' +
                    '<span class="glyphicon glyphicon-remove del-m-wafer"></span>' +
                    '</div >'
                );
            }
       }

       $('body').on('click', '.del-m-wafer', function(){
            $(this).parent().remove();
       })
    }
    return {
        init: function(){
            show();
        }
    }
}();