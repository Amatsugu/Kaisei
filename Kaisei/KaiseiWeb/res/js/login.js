var main;
var verified;
var authorize;
var submitBtn;
var appId;
var authId;
$(document).ready(function(){
	var state = 0;
	main = $("#mainForm");
	var user = $("input[name = 'UserEmail']");
	var password = $("input[name='password']");
	var newpassword = $("input[name='newpassword']");
	var password2 = $("input[name='password2']");
	var username = $("input[name='username']");
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
	if(verified == true)
	{
		$.ajax({
			method: "GET",
			url: "auth/user",
		}).done(function(e){ //Skip to Authorize
			state = 2;
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
					username.prop("disabled", true);
					user.prop("disabled", true);
					password.prop("disabled", true);
					newpassword.prop("disabled", true);
					password2.prop("disabled", true);
					user.fadeOut(500, function()
					{
						state = 2; //Goto Logged in State
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
					user.prop("disabled", true);
					username.prop("disabled", true);
					password.prop("disabled", true);
					password2.prop("disabled", true);
					newpassword.prop("disabled", true);
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
				console.log("Redirecting");
			break;
		}
	});
});

function LoadAuthorize()
{
	if(appId == null || appId == "" || appId == undefined)
	{
		authId.prop("disabled", true);
		main.attr("method", "get");
		main.submit();
	}
	var userName = $("#authorize #user .name");
	var userDesc = $("#authorize #user .desc");
	var appName = $("#authorize #user .name");
	var appDesc = $("#authorize #user .desc");
	$.ajax({
		url: "/auth/user",
		method: "GET"
	}).done(function(e){
		console.log(e);
		userName.text(e.username);
		userDesc.text(e.email);
	});

	$.ajax({
		url: "/app/" + appId,
		method: "GET",
	}).done(function(e){
		console.log(e);
		appName.text(e.name);
		appDesc.text(e.description);
	});
	authorize.fadeIn(500);
	submitBtn.val("Authorize");
}