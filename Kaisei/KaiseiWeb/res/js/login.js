$(document).ready(function(){
	var state = 0;
	var main = $("#mainForm");
	var user = $("input[name = 'UserEmail']");
	var password = $("input[name='password']");
	var newpassword = $("input[name='newpassword']");
	var password2 = $("input[name='password2']");
	var username = $("input[name='username']");
	var error = $("label.error");
	var email = "";
	var apiUri = window.location.host;
	var pw = "";
	var un = "";
	main.on("submit", function(e){
		e.preventDefault();
		switch(state)
		{
			case 0: //User Email
				email = user.val();
				if(email == null || email == undefined || email == "")
				{
					error.text("Invalid Email");
					error.animate({	opacity: 1 }, 500);
					user.focus();
					return;
				}
				error.animate({ opacity: 0 }, 500);
				if(false){ //TODO: Check User
					password.fadeOut(500);
					user.fadeOut(500, function()
					{
						state = 3; //Goto Login State
						password.fadeIn(500);
						password.focus();
					});
				}
				else{
					error.html("User does not exist, <a href='#'>Register</a>");
					error.children("a").click(function(e){
						e.preventDefault();
						state = 1;
						error.animate({ opacity: 0 }, 500);
						password.fadeOut(500);
						user.fadeOut(500, function(){
							password.val("");
							newpassword.fadeIn(500);
							password2.fadeIn(500);
							newpassword.focus();
						});
					});
					error.animate({	opacity: 1 }, 500);
				}
				state++;
			break;
			case 1: //New User
				pw = newpassword.val();
				var p = password2.val();
				if(pw == null || pw == undefined || pw == "")
				{
					error.text("Password Must be 8-100 characters long");
					error.animate({	opacity: 1 }, 500);
					newpassword.focus();
					return;
				}
				if(p == null || p == undefined || p == "")
				{
					error.text("Password Must be 8-100 characters long");
					error.animate({	opacity: 1 }, 500);
					password2.focus();
					return;
				}else if(p != pw)
				{
					error.html("Passwords do not match");
					error.animate({	opacity: 1 }, 500);
					password2.focus();
					return;
				}
				error.animate({ opacity: 0 }, 500);
				password2.fadeOut(500);
				newpassword.fadeOut(500, function(){
					username.fadeIn(500);
					username.focus();
				});
				state++;
			break;
			case 2: //New User username
				un = username.val();
				if(un == null || un == undefined || un == "")
				{
					error.text("Usernames must be 3-100 characters long");
					error.animate({	opacity: 1 }, 500);
					username.focus();
					return;
				}
				error.animate({ opacity: 0 }, 500);
				username.fadeOut(500, function(){
				});
				state = 4;
			break;
			case 3: //Existing User Password
				
			break;
			case 4: //Finsih

			break;
		}
	});
});