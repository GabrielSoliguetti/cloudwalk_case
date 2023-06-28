'Importar a biblioteca Json.NET
Imports System.Windows
Imports System.Windows.Forms.DataFormats
Imports Newtonsoft.Json

'Declarar uma classe para desserializar o arquivo JSON


Public Class Form1
    Public Class Dados
        Public Property transaction_id As String
        Public Property merchant_id As String
        Public Property user_id As String
        Public Property card_number As String
        Public Property transaction_date As String
        Public Property transaction_amount As String
        Public Property device_id As String
        Public Property has_cbk As String
    End Class

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim caminho As String = txtCaminhoArquivoJson.Text

        'Verificar se o arquivo existe
        If IO.File.Exists(caminho) Then
            'Ler o conteúdo do arquivo JSON
            Dim json As String = IO.File.ReadAllText(caminho)

            'Desserializar o arquivo JSON em uma lista de objetos da classe Dados
            Dim lista As List(Of Dados) = JsonConvert.DeserializeObject(Of List(Of Dados))(json)

            'Vincular a fonte de dados da datagridview dgvDados à lista de objetos
            dgvDados.DataSource = lista

            'Opcional: ajustar as colunas da datagridview
            dgvDados.AutoResizeColumns()
        Else
            'Mostrar uma mensagem de erro se o arquivo não existir
            MessageBox.Show("Arquivo não encontrado: " & caminho, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub runVelocity_Click(sender As Object, e As EventArgs) Handles runVelocity.Click
        'Obter os valores das caixas de texto txtQtdTransactions e txtQtdDays
        Dim qtdTransactions As Integer = Integer.Parse(txtQtdTransactions.Text)
        Dim qtdDays As Integer = Integer.Parse(txtQtdDays.Text)

        'Criar uma lista para armazenar os registros bloqueados
        Dim blockedList As New List(Of Dados)

        'Percorrer as linhas da datagridview dgvDados
        For Each row As DataGridViewRow In dgvDados.Rows
            'Obter os valores das células da linha atual
            Dim transaction_id As String = row.Cells("transaction_id").Value.ToString()
            Dim user_id As String = row.Cells("user_id").Value.ToString()
            Dim card_number As String = row.Cells("card_number").Value.ToString()
            Dim transaction_date As Date = Date.Parse(row.Cells("transaction_date").Value.ToString())
            Dim transaction_amount As Decimal = Decimal.Parse(row.Cells("transaction_amount").Value.ToString())

            'Criar uma variável para contar o número de transações do mesmo usuário e cartão nos últimos dias
            Dim count As Integer = 0

            'Percorrer as outras linhas da datagridview dgvDados
            For Each otherRow As DataGridViewRow In dgvDados.Rows
                'Obter os valores das células da outra linha
                Dim other_user_id As String = otherRow.Cells("user_id").Value.ToString()
                Dim other_card_number As String = otherRow.Cells("card_number").Value.ToString()
                Dim other_transaction_date As Date = Date.Parse(otherRow.Cells("transaction_date").Value.ToString())

                'Verificar se o usuário e o cartão são os mesmos e se a data da transação está dentro do intervalo de dias
                If user_id = other_user_id AndAlso card_number = other_card_number AndAlso transaction_date.Subtract(other_transaction_date).TotalDays <= qtdDays Then
                    'Incrementar o contador de transações
                    count += 1
                End If
            Next

            'Verificar se o número de transações excede o limite definido na caixa de texto txtQtdTransactions
            If count > qtdTransactions Then
                'Criar um objeto da classe Dados com os valores da linha atual
                Dim dados As New Dados With {
                    .transaction_id = transaction_id,
                    .user_id = user_id,
                    .card_number = card_number,
                    .transaction_date = transaction_date,
                    .transaction_amount = transaction_amount
                }

                'Adicionar o objeto à lista de registros bloqueados
                blockedList.Add(dados)
            End If

        Next

        'Serializar a lista de registros bloqueados em um arquivo JSON chamado blocked_list.json
        Dim json As String = JsonConvert.SerializeObject(blockedList, Formatting.Indented)
        IO.File.WriteAllText("blocked_list.json", json)

        'Mostrar uma mensagem de confirmação se o arquivo foi criado com sucesso
        MessageBox.Show("Arquivo blocked_list.json criado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub blockedTransactions_Click(sender As Object, e As EventArgs) Handles blockedTransactions.Click
        'Obter o caminho do arquivo JSON chamado blocked_list.json
        Dim caminho As String = "blocked_list.json"

        'Verificar se o arquivo existe
        If IO.File.Exists(caminho) Then
            'Ler o conteúdo do arquivo JSON
            Dim json As String = IO.File.ReadAllText(caminho)

            'Desserializar o arquivo JSON em uma lista de objetos da classe Dados
            Dim lista As List(Of Dados) = JsonConvert.DeserializeObject(Of List(Of Dados))(json)

            'Instanciar um objeto da classe Form2 e passar a lista de objetos para a propriedade pública BlockedData
            Dim form2 As New BlockedList()
            form2.BlockedData = lista

            'Exibir o formulário Form2
            form2.Show()
        Else
            'Mostrar uma mensagem de erro se o arquivo não existir
            MessageBox.Show("Arquivo não encontrado: " & caminho, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub btn3DS_Click(sender As Object, e As EventArgs) Handles btn3DS.Click
        Dim amount As Decimal = Decimal.Parse(txtAmount.Text)

        'Criar uma lista para armazenar as transações com amount acima do valor inserido
        Dim list3DS As New List(Of Dados)

        'Percorrer as linhas da datagridview dgvDados
        For Each row As DataGridViewRow In dgvDados.Rows
            'Obter os valores das células da linha atual
            Dim transaction_id As String = row.Cells("transaction_id").Value.ToString()
            Dim user_id As String = row.Cells("user_id").Value.ToString()
            Dim card_number As String = row.Cells("card_number").Value.ToString()
            Dim transaction_date As Date = Date.Parse(row.Cells("transaction_date").Value.ToString())
            Dim transaction_amount As Decimal = Decimal.Parse(row.Cells("transaction_amount").Value.ToString())

            'Verificar se o valor da transação é maior que o valor inserido na caixa de texto txtAmount
            If transaction_amount > amount Then
                'Criar um objeto da classe Dados com os valores da linha atual
                Dim dados As New Dados With {
                    .transaction_id = transaction_id,
                    .user_id = user_id,
                    .card_number = card_number,
                    .transaction_date = transaction_date,
                    .transaction_amount = transaction_amount
                }

                'Adicionar o objeto à lista de transações com amount acima do valor inserido
                list3DS.Add(dados)
            End If

        Next

        'Serializar a lista de transações com amount acima do valor inserido em um arquivo JSON chamado 3DSTransactions.json
        Dim json As String = JsonConvert.SerializeObject(list3DS, Formatting.Indented)
        IO.File.WriteAllText("3DSTransactions.json", json)

        'Mostrar uma mensagem de confirmação se o arquivo foi criado com sucesso
        MessageBox.Show("Arquivo 3DSTransactions.json criado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        'Obter o caminho do arquivo JSON chamado blocked_list.json
        Dim caminho As String = "3DSTransactions.json"

        'Verificar se o arquivo existe
        If IO.File.Exists(caminho) Then
            'Ler o conteúdo do arquivo JSON
            Dim json As String = IO.File.ReadAllText(caminho)

            'Desserializar o arquivo JSON em uma lista de objetos da classe Dados
            Dim lista As List(Of Dados) = JsonConvert.DeserializeObject(Of List(Of Dados))(json)

            'Instanciar um objeto da classe Form2 e passar a lista de objetos para a propriedade pública BlockedData
            Dim form2 As New BlockedList()
            form2.BlockedData = lista

            'Exibir o formulário Form2
            form2.Show()
            form2.Text = "Sent to 3DS List"
        Else
            'Mostrar uma mensagem de erro se o arquivo não existir
            MessageBox.Show("Arquivo não encontrado: " & caminho, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub blockCBKUsers_Click(sender As Object, e As EventArgs) Handles blockCBKUsers.Click
        'Criar uma lista para armazenar as transações que tem a coluna has_cbk como True
        Dim listCBK As New List(Of Dados)

        'Percorrer as linhas da datagridview dgvDados
        For Each row As DataGridViewRow In dgvDados.Rows
            'Obter os valores das células da linha atual
            Dim transaction_id As String = row.Cells("transaction_id").Value.ToString()
            Dim user_id As String = row.Cells("user_id").Value.ToString()
            Dim card_number As String = row.Cells("card_number").Value.ToString()
            Dim transaction_date As Date = Date.Parse(row.Cells("transaction_date").Value.ToString())
            Dim transaction_amount As Decimal = Decimal.Parse(row.Cells("transaction_amount").Value.ToString())
            Dim has_cbk As Boolean = Boolean.Parse(row.Cells("has_cbk").Value.ToString())

            'Verificar se o valor da coluna has_cbk é True
            If has_cbk Then
                'Criar um objeto da classe Dados com os valores da linha atual
                Dim dados As New Dados With {
                    .transaction_id = transaction_id,
                    .user_id = user_id,
                    .card_number = card_number,
                    .transaction_date = transaction_date,
                    .transaction_amount = transaction_amount,
                    .has_cbk = has_cbk
                }

                'Adicionar o objeto à lista de transações que tem a coluna has_cbk como True
                listCBK.Add(dados)
            End If

        Next

        'Serializar a lista de transações que tem a coluna has_cbk como True em um arquivo JSON chamado blockCBKUsers.json
        Dim json As String = JsonConvert.SerializeObject(listCBK, Formatting.Indented)
        IO.File.WriteAllText("blockCBKUsers.json", json)

        'Mostrar uma mensagem de confirmação se o arquivo foi criado com sucesso
        MessageBox.Show("Arquivo blockCBKUsers.json criado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        'Obter o caminho do arquivo JSON chamado blocked_list.json
        Dim caminho As String = "blockCBKUsers.json"

        'Verificar se o arquivo existe
        If IO.File.Exists(caminho) Then
            'Ler o conteúdo do arquivo JSON
            Dim json As String = IO.File.ReadAllText(caminho)

            'Desserializar o arquivo JSON em uma lista de objetos da classe Dados
            Dim lista As List(Of Dados) = JsonConvert.DeserializeObject(Of List(Of Dados))(json)

            'Instanciar um objeto da classe Form2 e passar a lista de objetos para a propriedade pública BlockedData
            Dim form2 As New BlockedList()
            form2.BlockedData = lista

            'Exibir o formulário Form2
            form2.Show()
            form2.Text = "CBK Users"
        Else
            'Mostrar uma mensagem de erro se o arquivo não existir
            MessageBox.Show("Arquivo não encontrado: " & caminho, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub
End Class
