var Wafer = function(){
    var show = function () {
        var mywafertable = null;

       $('#marks').focus();

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
        
       function RefreshWaferTable(warning)
       {
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
            if (cur_marks.length === 0)
            {
                if (warning)
                {
                    alert("查询条件不可为空！");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
             $.post('/WaferPack/QueryWafer', 
             {
                 marks: JSON.stringify(cur_marks)
             }, function(output){
                 var idx = 0;
                 var datacont = output.data.length;

                 if (mywafertable)
                 {
                     mywafertable.destroy();
                 }
                 $("#WaferTableID").empty();

                 for (idx = 0; idx < datacont; idx++)
                 {
                     var line = output.data[idx];
                     if (line.Status === "NG") {
                         $("#WaferTableID").append('<tr class="danger"><td>' + line.SN + '</td><td>' + line.DateCode + '</td><td>' + line.WaferNum + '</td><td>' + line.PN + '</td><td>' + line.Status + '</td></tr>');
                     }
                     else {
                         $("#WaferTableID").append('<tr><td>' + line.SN + '</td><td>' + line.DateCode + '</td><td>' + line.WaferNum + '</td><td>' + line.PN + '</td><td>' + line.Status + '</td></tr>');
                     }
                 }

                 mywafertable = $('#mywafertable').DataTable({
                     'iDisplayLength': 50,
                     'aLengthMenu': [[20, 50, 100, -1],
                     [20, 50, 100, "All"]],
                     "aaSorting": [],
                     "order": []
                 });

                 if (output.waferdup)
                 {
                     alert('含有不同WAFER的DATECODE: ' + output.waferdup);
                 }

                 $('#myLoadingModal').modal('hide');
             })
       }


       $('body').on('click', '#btn-marks-submit', function(){
           RefreshWaferTable(true);
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
                    '<span class="wafer-no-list">' + wafer_no + '</span>' +
                    '<span class="glyphicon glyphicon-remove del-m-wafer"></span>' +
                    '</div >'
                );
            }
       }

       $('body').on('click', '.del-m-wafer', function(){
           $(this).parent().remove();
       })
        
       $('body').on('click', '#m-save-setting', function () {
           var wafer_list = new Array();
           $.each($('.wafer-no-list'), function () {
               wafer_list.push($.trim($(this).html()));
           })

           $.post('/WaferPack/UpdateNGWafer',
               {
                   wafer_list: JSON.stringify(wafer_list)
               },
               function () {
                   $('#modal-op-setting').modal('hide');
                   RefreshWaferTable(false);
           });
       })

       $('body').on('click', '.op-download', function () {
           var all_marks = $.trim($('#marks').val()).split('\n');
           var cur_marks = new Array();
           var arr_count = new Array();
           $.each(all_marks, function (i, val) {
               if (val != "") {
                   if (arr_count[val]) {
                       arr_count[val]++;
                   }
                   else {
                       arr_count[val] = 1;
                       cur_marks.push(val);
                   }
               }
           })
           if (cur_marks.length === 0) {
               alert("查询条件不可为空！");
               return false;
           }
           $('#marks').val(cur_marks.join('\n'));
           $.post('/WaferPack/DownloadWaferData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               window.open(output.data, '_blank');
           })
       })
    }
    return {
        init: function(){
            show();
        }
    }
}();