namespace BlazorBUnit.Tests
{
    public class TestComponentTests : TestContext
    {
        [Fact]
        public void GivenAValidComponent_WhenComponentIsRendered_ThenMarkupMatchesExpectedOutput()
        {
            // Arrange & Act
            var cut = RenderComponent<TestComponent>();

            // Assert
            cut.MarkupMatches("<h3>TestComponent</h3>");
        }

        [Fact]
        public void GivenAComponentWithAParameter_WhenComponentIsRendered_ThenMarkupMatchesExpectedOutput()
        {
            // Arrange & Act
            var message = "This is a message";

            var cut = RenderComponent<TestComponentWithParameter>(parameters => parameters.Add(p => p.Message, message));

            // Assert
            cut.MarkupMatches($"<p>Message: {message}</p>");
        }

        [Fact]
        public void GivenAComponentWithCascadingParamter_WhenButtonIsClicked_ShouldUseCascadingIncrement()
        {
            // Arrange
            IRenderedComponent<TestComponentWithCascadingParameter>? cut = RenderComponent<TestComponentWithCascadingParameter>(parameters =>
              parameters.AddCascadingValue("Increment", 3)
            );

            // Act
            cut.Find(cssSelector: "button")
               .Click();

            // Assert
            cut.Find(cssSelector: "p")
               .MarkupMatches(@"<p>Current count: 3</p>");
        }

        [Fact]
        public void GivenAComponentWithEventHandler_WhenEventHandlerIsExecuted_ThenMarkupMatchesExpectedOutput()
        {
            // Arrange & Act
            var cut = RenderComponent<TestComponentWithEventHandler>();

            // Assert
            cut.Find("button").Click(ctrlKey: true);

            cut.Find("p").MarkupMatches("<p>Control key pressed: True</p>");
        }

        [Fact]
        public void GivenAComponentWithInjectedHttpClient_WhenComponentIsRendered_ThenHttpClientRetrievesExpectedData()
        {
            // Arrange
            var content = JsonSerializer.Serialize(new List<string> { "data" });

            var mockHttp = new MockHttpMessageHandler();
            var httpClient = mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost");

            Services.AddSingleton(httpClient);

            mockHttp.When("/api/data")
                    .Respond(HttpStatusCode.OK, "application/json", content);

            // Act
            var cut = RenderComponent<TestComponentWithHttpClient>();

            // Assert
            cut.WaitForAssertion(() => Assert.NotNull(cut.Instance.DataFromApi));
        }

        [Fact]
        public void GivenAComponentWithInjectedService_WhenComponentIsRendered_ThenServiceRetrievesExpectedData()
        {
            // Arrange
            Services.AddSingleton<IMyDataService, MyDataService>();

            // Act
            var cut = RenderComponent<TestComponentWithInjection>();

            // Assert
            Assert.NotNull(cut.Instance.MyData);
        }

        [Fact]
        public void GivenAComponentWithJSInterop_WhenButtonIsClicked_ThenJSFunctionExecutes()
        {
            // Arrange
            JSInterop.SetupVoid("alert", "Alert from Blazor component");

            // Act
            var cut = RenderComponent<TestComponentWithJSInterop>();

            cut.Find("button").Click();

            // Assert
            JSInterop.VerifyInvoke("alert", calledTimes: 1);
        }

        [Fact]
        public void GivenAComponentWithNavigationManager_WhenButtonIsClicked_ThenNavigationManagerNavigatesToExpectedUri()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<FakeNavigationManager>();
            var cut = RenderComponent<TestComponentWithNavigationManager>();

            // Act
            cut.Find("button").Click();

            // Assert
            Assert.Equal($"{navigationManager.BaseUri}home", navigationManager.Uri);
        }
    }
}