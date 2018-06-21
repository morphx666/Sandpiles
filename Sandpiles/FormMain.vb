Imports System.Threading

' https://en.wikipedia.org/wiki/Abelian_sandpile_model

Public Class FormMain
    Private initValue As ULong = 0
    Private maxValue As ULong = 4
    Private gridSize As Integer = 2
    'Private colors() = {Color.Blue, Color.Cyan, Color.CadetBlue, Color.LightBlue}
    'Private colors() = {Color.Black, Color.Yellow, Color.Blue, Color.Red}
    Private colors() = {Color.Blue, Color.Cyan, Color.Yellow, Color.DarkRed}
    ' --------------------------------

    Private bmp As DirectBitmap
    Private zoom As Double = 1
    Private xOff As Integer = 0
    Private yOff As Integer = 0
    Private mousePos As Point
    Private isDragging As Boolean

    Private primaryBuffer()() As ULong
    Private secondaryBuffer()() As ULong

    Private w As Integer
    Private h As Integer

    Private si As Integer
    Private sj As Integer
    Private mi As Integer
    Private mj As Integer

    Private autoRefresh As Thread
    Private runAlgorithm As Thread

    Private Sub Sandpiles_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        InitBuffers()

        AddHandler Me.SizeChanged, Sub() InitBuffers()
        AddHandler Me.MouseWheel, Sub(s1 As Object, e1 As MouseEventArgs)
                                      zoom += e1.Delta / 2000
                                      zoom = Math.Max(0.01, Math.Min(8, zoom))
                                  End Sub
        AddHandler Me.MouseDown, Sub(s1 As Object, e1 As MouseEventArgs)
                                     mousePos = e1.Location
                                     isDragging = True
                                 End Sub
        AddHandler Me.MouseMove, Sub(s1 As Object, e1 As MouseEventArgs)
                                     If isDragging Then
                                         xOff += (e1.X - mousePos.X) / zoom
                                         yOff += (e1.Y - mousePos.Y) / zoom
                                         mousePos = e1.Location
                                     End If
                                 End Sub
        AddHandler Me.MouseUp, Sub() isDragging = False
        AddHandler Me.DoubleClick, Sub()
                                       xOff = 0
                                       yOff = 0
                                       zoom = 1
                                   End Sub
    End Sub

    Private Sub StartThreads()
        autoRefresh = New Thread(Sub()
                                     Do
                                         Thread.Sleep(30)
                                         RenderBuffer()
                                         Me.Invalidate()
                                     Loop
                                 End Sub) With {.IsBackground = True}
        autoRefresh.Start()

        runAlgorithm = New Thread(Sub()
                                      Do
                                          SetMaximumsCache()

                                          For i As Integer = si To mi
                                              For j As Integer = sj To mj
                                                  If primaryBuffer(i)(j) >= maxValue Then
                                                      secondaryBuffer(i)(j) += (primaryBuffer(i)(j) - maxValue)

                                                      If i - 1 >= 0 Then secondaryBuffer(i - 1)(j) += 1 : If i - 1 < si Then si = i - 1
                                                      If i + 1 < w Then secondaryBuffer(i + 1)(j) += 1
                                                      If j - 1 >= 0 Then secondaryBuffer(i)(j - 1) += 1 : If j - 1 < sj Then sj = j - 1
                                                      If j + 1 < h Then secondaryBuffer(i)(j + 1) += 1
                                                  End If
                                              Next
                                          Next

                                          SetMaximumsCache()

                                          For i As Integer = si To mi
                                              For j As Integer = sj To mj
                                                  primaryBuffer(i)(j) = secondaryBuffer(i)(j)
                                                  If secondaryBuffer(i)(j) >= maxValue Then secondaryBuffer(i)(j) = maxValue - 4
                                              Next
                                          Next
                                      Loop
                                  End Sub) With {.IsBackground = True}
        runAlgorithm.Start()
    End Sub

    Private Sub InitBuffers()
        Static lastW As Integer = 0
        Static lastH As Integer = 0

        Dim tmpW As Integer = Me.DisplayRectangle.Width \ gridSize
        Dim tmpH As Integer = Me.DisplayRectangle.Height \ gridSize

        If lastW <> tmpW OrElse lastH <> tmpH Then
            If autoRefresh IsNot Nothing Then
                autoRefresh.Abort()
                runAlgorithm.Abort()
            End If

            w = tmpW
            h = tmpH
            lastW = w
            lastH = h

            ReDim primaryBuffer(w - 1)
            ReDim secondaryBuffer(w - 1)
            For x As Integer = 0 To w - 1
                ReDim primaryBuffer(x)(h - 1)
                ReDim secondaryBuffer(x)(h - 1)
            Next

            bmp = New Bitmap(w * gridSize, h * gridSize)
            Me.BackColor = colors(0)

            For i As Integer = 0 To w - 1
                For j As Integer = 0 To h - 1
                    secondaryBuffer(i)(j) = initValue
                Next
            Next
            primaryBuffer(w \ 2)(h \ 2) = 28000000

            si = w \ 2 - 1
            sj = h \ 2 - 1
            SetMaximumsCache()

            StartThreads()
        End If
    End Sub

    Private Sub RenderBuffer()
        Dim c As Color
        Dim n As Integer

        For i As Integer = si To mi
            For j As Integer = sj To mj
                n = primaryBuffer(i)(j)
                If n >= colors.Length Then
                    c = colors(3)
                Else
                    c = colors(n)
                End If

                bmp.FillRectangle(c, i * gridSize, j * gridSize, gridSize, gridSize)
            Next
        Next

        'Me.Invoke(New MethodInvoker(Sub() Me.Text = primaryBuffer(w \ 2)(h \ 2).ToString("N0")))
    End Sub

    Private Sub SetMaximumsCache()
        mi = If(si = 0, w - 1, w - si)
        mj = If(sj = 0, h - 1, h - sj)
    End Sub

    Private Sub FormMain_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        If zoom <> 1 Then
            e.Graphics.TranslateTransform((1 - zoom) * bmp.Width / 2, (1 - zoom) * bmp.Height / 2)
            e.Graphics.ScaleTransform(zoom, zoom)
        End If
        e.Graphics.TranslateTransform(xOff, yOff)
        e.Graphics.DrawImageUnscaled(bmp, 0, 0)

        'e.Graphics.DrawRectangle(Pens.White, si * gridSize, sj * gridSize, 2 * (w \ 2 - si) * gridSize, 2 * (h \ 2 - sj) * gridSize)
    End Sub
End Class
