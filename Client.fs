namespace DUE_FSharp_SPASandbox_2026

open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "">] Home

[<JavaScript>]
module Client =

    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    type StudyTask =
        {
            Id: int
            Text: string
        }

    let StudyTasks =
        ListModel.Create (fun task -> task.Id) [
            { Id = 1; Text = "Matematika gyakorlás" }
            { Id = 2; Text = "Hálózatok jegyzet átnézése" }
        ]

    let nextTaskId = Var.Create 3

    let router = Router.Infer<EndPoint>()
    let currentPage = Router.InstallHash EndPoint.Home router

    module Pages =

        let Home () =
            let newTask = Var.Create ""

            div [attr.``class`` "bg-white rounded-xl shadow-md p-8 space-y-6"] [
                div [attr.``class`` "space-y-3"] [
                    h1 [attr.``class`` "text-3xl font-bold text-gray-800"] [
                        text "Smart Study Planner"
                    ]
                    p [attr.``class`` "text-gray-600"] [
                        text "Add your study tasks for today and keep track of your learning progress."
                    ]
                ]

                div [attr.``class`` "bg-gray-50 rounded-lg p-4 border border-gray-200 space-y-4"] [
                    h2 [attr.``class`` "text-xl font-semibold text-gray-800"] [
                        text "Study task list"
                    ]

                    div [attr.``class`` "flex flex-col md:flex-row gap-3"] [
                        Doc.Input [
                            attr.``class`` "flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400"
                            attr.placeholder "Enter a study task"
                        ] newTask

                        button [
                            attr.``class`` "bg-blue-600 hover:bg-blue-700 text-white font-semibold px-5 py-2 rounded-lg"
                            on.click (fun _ _ ->
                                let taskText = newTask.Value.Trim()
                                if taskText <> "" then
                                    StudyTasks.Add {
                                        Id = nextTaskId.Value
                                        Text = taskText
                                    }
                                    nextTaskId.Value <- nextTaskId.Value + 1
                                    newTask.Value <- ""
                            )
                        ] [
                            text "Add task"
                        ]
                    ]

                    div [attr.``class`` "space-y-2"] [
                        StudyTasks.View.DocSeqCached(fun task ->
                            div [attr.``class`` "bg-white border border-gray-200 rounded-lg px-4 py-3 shadow-sm flex items-center justify-between gap-3"] [
                                div [] [
                                    text task.Text
                                ]

                                button [
                                    attr.``class`` "bg-red-500 hover:bg-red-600 text-white font-semibold px-4 py-2 rounded-lg"
                                    on.click (fun _ _ ->
                                        StudyTasks.RemoveByKey task.Id
                                    )
                                ] [
                                    text "Delete"
                                ]
                            ]
                        )
                    ]
                ]

                div [attr.``class`` "grid md:grid-cols-3 gap-4"] [
                    div [attr.``class`` "bg-blue-50 rounded-lg p-4 border border-blue-100"] [
                        h2 [attr.``class`` "text-lg font-semibold text-gray-800"] [
                            text "Track tasks"
                        ]
                        p [attr.``class`` "text-sm text-gray-600 mt-2"] [
                            text "Keep your current study tasks in one simple list."
                        ]
                    ]

                    div [attr.``class`` "bg-green-50 rounded-lg p-4 border border-green-100"] [
                        h2 [attr.``class`` "text-lg font-semibold text-gray-800"] [
                            text "Stay focused"
                        ]
                        p [attr.``class`` "text-sm text-gray-600 mt-2"] [
                            text "Add one task at a time and build a daily learning routine."
                        ]
                    ]

                    div [attr.``class`` "bg-yellow-50 rounded-lg p-4 border border-yellow-100"] [
                        h2 [attr.``class`` "text-lg font-semibold text-gray-800"] [
                            text "Plan ahead"
                        ]
                        p [attr.``class`` "text-sm text-gray-600 mt-2"] [
                            text "This project will later support deadlines and study planning."
                        ]
                    ]
                ]

                div [attr.``class`` "border-t pt-4"] [
                    p [attr.``class`` "text-sm text-gray-500"] [
                        text "Version 3: task deletion"
                    ]
                    p [attr.``class`` "text-sm text-gray-500"] [
                        text "Status: in progress"
                    ]
                ]
            ]

    [<SPAEntryPoint>]
    let Main () =

        let renderInnerPage (currentPage: Var<EndPoint>) =
            currentPage.View.Map(fun endpoint ->
                match endpoint with
                | Home -> Pages.Home()
            )
            |> Doc.EmbedView

        IndexTemplate()
            .Content(renderInnerPage currentPage)
            .Bind()