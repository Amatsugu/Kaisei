var main;
var verified;
var authorize;
var submitBtn;
var appId;
var authId;
var username, user, password, password, newpassword;
$(document).ready(function(){
	var state = 0;
	main = $("#mainForm");
	user = $("input[name = 'UserEmail']");
	password = $("input[name='password']");
	newpassword = $("input[name='newpassword']");
	password2 = $("input[name='password2']");
	username = $("input[name='username']");
	var error = $("label.error");
	var pw = "";
	var un = "";
	submitBtn = $("input[type=submit]");
	authorize = $("#authorize");
	verified = main.data("verified");
	appId = main.data("appid");
	authId = $("input[name='authId']");
	/*user.fadeOut(500);
	password.fadeOut(500, function(){
		LoadAuthorize();
	});
	return;*/
	if(verified == "True")
	{
		$.ajax({
			method: "GET",
			url: "/user"
		}).done(function(e){ //Skip to Authorize
			state = 2;
			// console.log(e);
			user.fadeOut(500);
			password.fadeOut(500, function(){
				LoadAuthorize();
			});
		}).fail(function(e){
			verified = false;
		});
	}
	main.on("submit", function(e){
		error.animate({opacity : 0 }, 500);
		switch(state)
		{
			case 0: //User Login
				e.preventDefault();

				$.ajax({
					method: "POST",
					url: "/login",
					data: {
						Email: user.val(),
						Password: password.val()
					}
				}).done(function(){
					console.log("Login");
					password.fadeOut(500);
					user.fadeOut(500, function()
					{
						state = 3; //Goto Logged in State
						LoadAuthorize();
					});
				}).fail(function(){
					console.log("Login Failed");
					error.html("User does not exist, <a href='#'>Register</a>");
					error.children("a").click(function(e){
						e.preventDefault();
						state = 1; //Goto Register State
						error.animate({ opacity: 0 }, 500);
						password.fadeOut(500);
						user.fadeOut(500, function(){
							password.val("");
							newpassword.val("");
							password2.val("");
							username.val("");
							username.fadeIn(500);
							username.focus();
							newpassword.fadeIn(500);
							password2.fadeIn(500);
						});
					});
					error.animate({	opacity: 1 }, 500);
				});
			break;
			case 1: //Register
				e.preventDefault();
				//Username
				un = username.val();
				if(un == null || un == undefined || un == "")
				{
					error.text("Usernames must be 3-100 characters long");
					error.animate({	opacity: 1 }, 500);
					username.focus();
					return;
				}
				//Password
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
				$.ajax({
					method: "POST",
					url: "/login/register",
					data: {
						Email: user.val(),
						Username: un,
						Password: pw
					}
				}).done(function(){ //Registration Success
					//Transitions
					error.animate({ opacity: 0 }, 500);
					username.fadeOut(500);
					password2.fadeOut(500);
					newpassword.fadeOut(500, function(){
						state = 2;
						LoadAuthorize();
					});
				}).fail(function(){
					//TODO: Add error message
				});
				
			break;
			case 2: //Authorize
				CleanForm();
				e.preventDefault();
				$.ajax({
					url: "/auth/sso",
					method : "POST",
					data: {
						appId : appId
					}
				}).done(function(e)
				{
					state = 3;
					// console.log(e);
					authId.val(e);
					main.submit();
				}).fail(function(e)
				{
					console.log(e);
				});
			break;
			case 3: //Redirect
				console.log("Redirecting");
			break;
		}
	});
});

function CleanForm()
{
	user.remove();
	username.remove();
	password.remove();
	password2.remove();
	newpassword.remove();
}

function LoadAuthorize()
{
	if(appId == null || appId == "" || appId == undefined)
	{
		CleanForm();
		state = 3;
		authId.remove();
		main.attr("method", "get");
		main.submit();
		return;
	}
	$("#user .icon").attr("src", "/user/avatar");
	$.ajax({
		url: "/user/isAuth",
		data: {
			AppId : appId
		}
	}).done(function(e){
		console.log(e);
		if(e == true)
		{
			CleanForm();
			main.prepend("You have already authorized this app!");
			//TODO: Give SSO Login
			submitBtn.remove();
		}
	});

	var userName = $("#authorize #user .name");
	var userDesc = $("#authorize #user .desc");
	var appName = $("#authorize #app .name");
	var appDesc = $("#authorize #app .desc");
	$.ajax({
		url: "/user",
		method: "GET"
	}).done(function(e){
		// console.log(e);
		userName.text(e.username);
		userDesc.text(e.email);
	});

	$.ajax({
		url: "/app/" + appId,
		method: "GET",
	}).done(function(e){
		// console.log(e);
		appName.text(e.name);
		appDesc.text(e.description);
	});
	authorize.fadeIn(500);
	submitBtn.val("Authorize");
}