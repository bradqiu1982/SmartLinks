var Exam = function(){
    var show = function(){

        $('body').on('click', '#m-btn-submit', function(){
            var video_id = $.trim($('#tactivevid').val());
            var uname = $.trim($('#m-uname').val());
            if(uname == ''){
                alert("Please input your name");
                return false;
            }
            var answers = new Array();
            var msg = "";
            $('.question').each(function(){
                var q_id = $(this).data('id');
                var q_type = $(this).data('type');
                var answer = '';
                if (q_type == '' || q_type == 'SINGLE') {
                    if($(this).next().find('input[type=radio]:checked').length != 0){
                        answer = answer + $(this).next().find('input[type=radio]:checked').val() + ',';
                    }
                }
                else if (q_type == 'MULTI') {
                    if($(this).next().find('input[type=checkbox]:checked').length != 0){
                        $(this).next().find('input[type=checkbox]:checked').each(function(){
                            answer = answer + $(this).val()+',';
                        });
                    }
                }
                if(answer == ''){
                    msg += $(this).children().eq(0).html() + $(this).children().eq(1).html();
                    return false;
                }
                var ans_tmp = {
                    'q_id': q_id,
                    'q_type': q_type,
                    'answer' : answer
                }
                answers.push(ans_tmp);
            })
            if(msg != ""){
                alert("Please select your answer for: " + msg);
                return false;
            }
            //var output = {
            //    'score' : 80,
            //    'answers': [
            //        {
            //            'q_id': 1,
            //            'q_type': 'SINGLE',
            //            'answer': 'A',
            //            'c_answer': 'B'
            //        },
            //        {
            //            'q_id': 2,
            //            'q_type': 'SINGLE',
            //            'answer': 'B',
            //            'c_answer': 'B'
            //        },
            //        {
            //            'q_id': 3,
            //            'q_type': 'SINGLE',
            //            'answer': 'D',
            //            'c_answer': 'B'
            //        },
            //        {
            //            'q_id': 4,
            //            'q_type': 'SINGLE',
            //            'answer': 'A,D',
            //            'c_answer': 'B,D'
            //        }
            //    ]
            //}
            $.post('/TVideoSite/CheckUserAnswer',
             {
                 video_id: video_id,
                 uname: uname,
                 data : JSON.stringify(answers)
             }, function(output){
                $('input[type=text]').attr('disabled', 'disabled');
                $('input[type=radio]').attr('disabled', 'disabled');
                $('input[type=checkbox]').attr('disabled', 'disabled');
                $('.c_score').html("Score: "+output.score);
                $(output.answers).each(function(){
                    var $ans_tmp = $('.question[data-id='+this.q_id+']').next();
                    var append_str = '<div class="answer">'+
                            '<label>正确答案：</label>'+
                            '<label>'+this.answer+'</label>'+
                        '</div>';
                    $ans_tmp.append(append_str);
                    $ans_tmp.find('input').prop('checked', false);
                    var that = this;
                    if (that.q_type == '' || that.q_type == 'SINGLE') {
                        $($ans_tmp.children()).each(function(){
                            if($(this).find('input').val() == that.uanswer){
                                $(this).find('input').prop('checked', true);
                                if(that.uanswer == that.answer){
                                    append_str = '<span class="glyphicon glyphicon-ok right-ans"></span>';
                                }
                                else{
                                    append_str = '<span class="glyphicon glyphicon-remove wrong-ans"></span>';
                                }
                                $(this).append(append_str);
                            }
                        })
                    }
                    else if(that.q_type == 'MULTI'){
                        var uanswers = that.uanswer.split(',');
                        var answers = that.answer.split(',');
                        $(uanswers).each(function(i, val){
                            $($ans_tmp.children()).find('input[value='+val+']').prop('checked', true);
                            if($.inArray(val, answers) != -1){
                                append_str = '<span class="glyphicon glyphicon-ok right-ans"></span>';
                            }
                            else{
                                append_str = '<span class="glyphicon glyphicon-remove wrong-ans"></span>';
                            }
                            $($ans_tmp.children()).find('input[value='+val+']').parent().append(append_str);
                        });
                    }
                })
                $('#m-btn-submit').val('Closed');
                $('#m-btn-submit').attr('id', 'm-btn-closed');
                $('#m-btn-submit').attr('name', 'm-btn-closed');
             })
        })
        
        $('body').on('click', '#m-btn-closed', function(){
            $('#modal-exam').modal('hide');
            //$('.vtest-img-div').addClass('hide');
        })
    }
    return{
        init: function(){
            show();
        }
    }
}();