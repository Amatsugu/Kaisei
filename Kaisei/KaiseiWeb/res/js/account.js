var authAppList, myApps, statsBox;
var icon, username, email, password;
var iconForm, iconProgress, upMsg;
var windowOverlay, windowCloseBtn, windowContent;
var curAppId;
$(document).ready(function(){
	authAppList = $("#authAppList");
	myApps = $("#myApps");
	iconForm = $("form")[0];
	iconProgress = $(".progress");
	//Icon
	icon = $("#changeIcon").prev().prev();
	upMsg = $("#uploadMessage");
	$("#myAppsHead button").click(function(){
		OpenWindow("Create App", RenderAppCreate)
	});
	//Window
	windowOverlay = $("#windowOverlay").click(function(e)
	{
		if(e.target == this)
			CloseWindow();
	});
	windowContent = $("#windowOverlay #window #windowBody");
	windowTitle = $("#windowOverlay #window #header");
	windowCloseBtn = $("#windowOverlay #window button").click(function(e){
		CloseWindow();
	});

	//Upload Icon
	$("form input").dmUploader({
		url: "/user/avatar",
		dnd: false,
		multiple:false,
		allowedTypes: "image/*",
		maxFileSize: 5e6,
		onUploadProgress: function (p) {
			iconProgress.css({
				width: p + "%"
			});
		},
		onUploadComplete: function () {
			console.log("complete");
			iconProgress.show();
			iconProgress.css({
				width: 100 + "%"
			});
			upMsg.text("Uploaded!");
			upMsg.fadeIn(500, function () {
				upMsg.delay(1000).fadeOut(500);
				iconProgress.delay(1000).fadeOut(500, function () {
					iconProgress.css({
						width: 0 + "%"
					});
				});
				icon.attr("src", "/user/avatar?m=" + new Date().getTime());
			});
		},
		onUploadError: function (xhr, status, err) {
			console.log("failed " + status);
			iconProgress.css({
				width: 100 + "%"
			});
			upMsg.text("Uploaded Failed");
			upMsg.fadeIn(500, function () {
				upMsg.delay(1000).fadeOut(500);
				iconProgress.delay(1000).fadeOut(500, function () {
					iconProgress.css({
						width: 0 + "%"
					});
				});
			});
		},
		onDragEnter: function () {
			upMsg.text = "Upload";
			upMsg.fadeIn(500);	
		},
		onDragLeave: function () {
			upMsg.fadeOut(500);	
		},
	});
	//Username Change
	username = $("#username").click(function(e){
		OpenWindow("Edit Info", RenderUserEditor);
	}).next();
	//Email Change
	email = $("#email").click(function(e){
		OpenWindow("Edit Info", RenderUserEditor);
	}).next();
	//Password Change
	password = $("#password").click(function(e){
		OpenWindow("Edit Info", RenderUserEditor);
	}).next();

	statsBox = $("#statsBox");

	//Load Apps
	$.ajax({
		url: "/user/authedapps",
		method: "GET"
	}).done(function(e){
		// console.log(e);
		authAppList.text("");
		// if(e.length > 0)
		AddStat("Authorized Apps", e.length);
		ListApps(e, authAppList, "Revoke", RevokeApp);
	}).fail(function(e){
		authAppList.text("Failed to load apps");
	});

	$.ajax({
		url: "user/apps",
		method: "GET"
	}).done(function(e){
		// console.log(e);
		myApps.text("");
		AddStat("My Apps", e.length);
		ListApps(e, myApps, "Edit", EditApp);
	}).fail(function(e){
		myApps.text("Failed to load apps");
	});;
});

function OpenWindow(title, renderContent)
{
	windowTitle.text(title);
	windowOverlay.fadeIn(500);
	renderContent();
}

function RenderAppCreate()
{
	var form = $("<form></form>").appendTo(windowContent);
	var err = $("<label class='error'></label").appendTo(form).text("Error").hide();
	$("<h3></h3").appendTo(form).text("App Info");
	$("<label></label").appendTo(form).text("App Name");
	var appName = $("<input type='text' placeholder='App Name' autocomplete='off' maxlength='100' required>").appendTo(form);
	$("<label></label").appendTo(form).text("App Description");
	var appDesc = $("<textarea rows='5' placeholder='App Description' maxlength='500' required>").appendTo(form);
	$("<label></label").appendTo(form).text("App Hostname");
	var appHost = $("<input type='url' placeholder='App Hostname' autocomplete='off' maxlength='100'>").appendTo(form);
	$("<label></label").appendTo(form).text("App Icon");
	var upZone = $("<div id='appIconUpload'></div>").appendTo(form);
	var appIcon = $("<input type='file' accept='image/*'>").appendTo(upZone).on("change", function () {
		var file = this.files[0];
		console.log(file);
		upText.text("\""+file.name+"\"");
	});
	$("<div id='uploadIcon'></div>").appendTo(upZone);
	var upText = $("<div id='uploadText'></div>").text("Choose Icon").appendTo(upZone);
	

	$("<button type='sumbit'></button>").appendTo(form).text("Create");

}

function RenderUserEditor()
{
	var form = $("<form></form>").appendTo(windowContent);
	var err = $("<label class='error'></label").appendTo(form).text("Error").hide();
	$("<h3></h3").appendTo(form).text("Change Account Info");
	$("<label></label").appendTo(form).text("Username");
	var unInput = $("<input type='text' name='username' placeholder='username' autocomplete='off' maxlength='100' value='"+username.text()+"' required>").appendTo(form);
	$("<label>Email</label").appendTo(form).text("Email");
	var emInput = $("<input type='email' name='UserEmail' placeholder='Email' autocomplete='off' maxlength='100' value='"+email.text()+"'> required").appendTo(form);
	$("<h3></h3").appendTo(form).text("Change Password");
	var pwInput = $("<input type='password' name='password' placeholder='Current Password' autocomplete='off' maxlength='100' value='' >").appendTo(form);
	var npwInput = $("<input type='password' name='newPassword' placeholder='New Password' autocomplete='off' maxlength='100' value=''>").appendTo(form);
	var npw2Input = $("<input type='password' name='newPassword2' placeholder='Confirm New Password' autocomplete='off' maxlength='100' value=''>").appendTo(form);
	$("<button type='sumbit'></button>").appendTo(form).text("Save");
	form.on("submit", function(e){
		e.preventDefault();
		var data = {};
		var submited = false;
		if(unInput.val() != username.text())
			data.Username = unInput.val();
		if(emInput.val() != email.text())
			data.Email = emInput.val();
		if(pwInput.val() != "" && npwInput.val() != "")
		{
			$.ajax({
				url:"/auth/verifyPassword",
				method: "POST",
				data:{
					Email : data.Email,
					Password : pwInput.val()
				}
			}).done(function(){
				err.fadeOut(500);
				if(npwInput.val() == npw2Input.val())
				{
					data.Password = npwInput.val();
					submited = true;
					SubmitUserChanges(data, err);
				}

			}).fail(function(){
				err.text("Your current password is incorrect");	
				err.fadeIn(500);
			});
		}
		console.log(data);
		if(!submited)
			SubmitUserChanges(data, err);
	});
}

function SubmitUserChanges(data, err)
{
	$.ajax({
		url: "/auth/updateUser",
		method: "POST",
		data: data
	}).done(function(){
		CloseWindow();
		username.text(data.Username);
		email.text(data.Email);
		err.fadeOut(500);
	}).fail(function(){
		err.text("Unable to update user info");
		err.fadeIn(500);
	});
}

function CloseWindow()
{
	windowOverlay.fadeOut(500, function () {
		windowContent.html("");
	});
}

function RevokeApp(appId, elem)
{
	var confirm = $("<div class='deleted' style='display:none'><p>Really?</p></div>").appendTo(elem);
	confirm.fadeIn(500);
	$("<button class='inline'>No</button>").appendTo(confirm).click(function(){
		confirm.fadeOut(500, function () {
			confirm.remove();
		});
	});
	$("<button class='inline'>Yes</button>").appendTo(confirm).click(function(){
		$.ajax({
			url: "/user/revokeApp",
			method: "POST",
			data:{
				AppId : appId
			}
		}).done(function(e){
			var del = $("<div class='deleted' style='display:none'><p>Deleted</p></div>").appendTo(elem);
			del.fadeIn(500, function () {
				elem.delay(1000).slideUp(500, function () {
					elem.remove();
				});
			});
		}).fail(function(e){
			var del = $("<div class='deleted' style='display:none'><p>Failed to Delete</p></div>").appendTo(elem);
			del.fadeIn(500).delay(1000).fadeOut(500);
		});
	});
}

function EditApp(appId, elem)
{
	
}
	
function AddStat(name, value) 
{
	var statBox = $("<div class='statBox' style='display:none'></div>").appendTo(statsBox);
	statBox.append("<div class='statName'>"+ name +"</div>");
	statBox.append("<div class='stat'>"+ value +"</div>");
	statBox.fadeIn(500);
}
	
function ListApps(apps, container, deleteMsg, deleteFunc)
{
	if(apps.length == 0)
	{
		container.text("No apps to display...");
		return;
	}
	container.html("");
	apps.forEach(app => {
		var appE = $("<div class='app'></div>").appendTo(container);
		appE.append("<img src='/app/avatar/" + app.id + "'>");
		appE.append("<div class='name'>" + app.name + "</div>");
		appE.append("<div class='host'><a href='http://" + app.hostname + "' target='_blank'>" + app.hostname + "</a></div>");
		$("<button>"+deleteMsg+"</button>").appendTo(appE).click(function(){
			deleteFunc(app.id, appE);
		});
		// appE.slideDown(500);
	});
}